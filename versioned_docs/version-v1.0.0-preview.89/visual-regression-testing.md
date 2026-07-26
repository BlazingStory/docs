# Visual Regression Testing

## Summary

A **visual regression test (VRT)** takes a screenshot of every story, keeps it as the baseline, and compares fresh screenshots against that baseline after you change something. If a component's appearance changes in a way nobody intended, the test fails before the change ships.

Once that safety net is in place, you can improve CSS aggressively, refactor markup, or let an AI coding agent rewrite a UI implementation, without the fear that an unnoticed visual breakage reaches your users.

Blazing Story supports this in two pieces:

- The Blazing Story app exposes a [**JavaScript API**](./javascript-api) in the browser, which reports every story it contains and tells a test runner when the view has settled down enough to be photographed.
- A project template named **"Blazing Story Visual Regression Test"** creates a ready-to-run VRT project that uses that API. It is a TypeScript project running on Node.js and [Playwright Test](https://playwright.dev/docs/intro), and it turns **every story in your app into one test with one screenshot, automatically**. You never register stories in the test project by hand, not even when you add new ones later.

The baseline screenshots are usually shared with your teammates and your CI/CD pipeline through a cloud storage service, so that everyone compares against the same, authoritative images. The project template supports Amazon S3, Azure Blob Storage, and Google Cloud Storage out of the box, and it can also scaffold an adapter for a storage service of your own.

## Requirements

- **.NET SDK ver.8 or later**, to install the project template and to run the Blazing Story app under test.
- **Node.js ver.24 or later**, to run the VRT project.
- A **running Blazing Story app**. The VRT project drives a real browser against a real running app, so the app must be up and reachable at the configured URL whenever the tests run.
- Depending on the storage service you pick, the **CLI of that service**, installed and authenticated (see [Choosing Where The Baseline Screenshots Live](#choosing-where-the-baseline-screenshots-live)).

## How It Works

It is worth knowing what happens during a test run, because it explains most of the behavior described later on this page.

1. Before every test run, the VRT project opens the Blazing Story app in a headless Chromium browser and calls the JavaScript API's `getStoryIndex()` to read the list of stories. The result is written to `tests/stories.json`.
2. Baseline screenshots that are missing on your machine are downloaded from the storage service, so a fresh clone or a clean CI agent still compares against the team's baselines.
3. The test file reads `tests/stories.json` and registers one Playwright test per story. This is why each story appears individually in the VS Code Test Explorer.
4. Each test navigates to that story's isolated preview page (`/iframe.html?id=<story id>&viewMode=story`), awaits the JavaScript API's `readyView()` so that rendering, fonts, and the opening transition have settled, and then captures the preview area with Playwright's [`toHaveScreenshot()`](https://playwright.dev/docs/test-snapshots).
5. Screenshots are compared against the baseline files in `tests/vrt.spec.ts-snapshots/`, and a story that renders differently fails the test.
6. The outcome is written as an HTML report at `playwright-report/index.html`.

Note that only **stories** are captured. "Docs" pages and custom pages appear in the story index, but they are not part of the visual regression test.

:::note
**Where the baselines live**  
`tests/vrt.spec.ts-snapshots/` is the local home of the baselines: it is where `npm run snapshots:pull` downloads into, and where `npm run snapshots:push` uploads from.

Each file there is named `<story id>-<platform>.png`, for example `example-button--primary-linux.png`. Because the platform is part of the file name, a screenshot taken on Linux is only ever compared against a Linux baseline, never against one captured on Windows or macOS. That is also why one storage location can hold the sets of several platforms side by side.
:::

## Preparation

Install the Blazing Story project templates, exactly as described in [Getting Started](./getting-started). If you have already done that for your Blazing Story app project, there is nothing more to install.

```shell
dotnet new install BlazingStory.ProjectTemplates
```

## Choosing Where The Baseline Screenshots Live

You choose the storage service when you create the VRT project, and that choice shapes the generated project (its dependencies, its npm scripts, and its README). Choose it before you create the project.

| Choice | Baselines are stored in | Authentication |
| --- | --- | --- |
| **AWS S3** | An Amazon S3 bucket | The AWS CLI (`aws configure`). The identity needs `s3:ListBucket`, `s3:GetObject`, `s3:PutObject`, and `s3:DeleteObject` on the bucket. |
| **Azure Blob Storage** | A blob container in an Azure Storage account | The Azure CLI (`az login`). The signed-in user needs the **"Storage Blob Data Contributor"** role on the storage account. Being a subscription Owner or Contributor is not enough. |
| **Google Cloud Storage** | A Google Cloud Storage bucket | The Google Cloud CLI. Run both `gcloud auth login` and `gcloud auth application-default login`, and set the project with `gcloud config set project <project id>`. |
| **Custom** | Any service you like | Up to the adapter you implement. See [Implementing A Custom Storage Adapter](#implementing-a-custom-storage-adapter). |
| **None** | The local disk only | Not applicable. |

:::note
Authentication is never configured inside the VRT project itself. The project relies on the credentials that the storage service's own CLI has already established on the machine, which is also what makes the same project work unchanged on a CI agent that has been logged in by other means.
:::

:::tip
"None" produces a VRT project whose baselines never leave your machine and are shared with nobody. That is a fine way to try visual regression testing out for the first time, but for team use, pick one of the storage services so that every developer and every CI run compares against the same baselines.
:::

:::warning
For an AWS S3 bucket, use the default SSE-S3 encryption. SSE-KMS changes the entity tag the sync uses to detect changed files, so the sync can no longer tell which baselines are up to date.
:::

## Creating A VRT Project

### On Visual Studio

Open the "Create a new project" dialog, select the **"Blazing Story Visual Regression Test"** project template, and fill in the dialog:

- **A base URL where the Blazing Story app under test is running**, for example `http://localhost:5000`.
- **A storage service for UI snapshots**, one of the choices in the table above.
- The connection values for the storage service you selected, for example the S3 region and bucket name. The dialog shows only the fields that apply to your choice.
- For the "Custom" storage service, which **AI coding agent** should receive the storage adapter skill (Claude Code, or the generic layout used by GitHub Copilot, OpenAI Codex, Cursor, and others), or none.

### On dotnet CLI

The template's short name is `blazingstoryvrt`. The base URL is required, and the remaining options depend on the storage service.

```shell
# AWS S3
dotnet new blazingstoryvrt -n MyBlazorWasmApp1.Stories.VRT \
  --base-url http://localhost:5000 \
  --storage aws --aws-region ap-northeast-1 --aws-bucket my-vrt-baselines

# Azure Blob Storage
dotnet new blazingstoryvrt -n MyBlazorWasmApp1.Stories.VRT \
  --base-url http://localhost:5000 \
  --storage azure --az-account mystorageaccount --az-container my-vrt-baselines

# Google Cloud Storage
dotnet new blazingstoryvrt -n MyBlazorWasmApp1.Stories.VRT \
  --base-url http://localhost:5000 \
  --storage gcp --gcp-bucket my-vrt-baselines

# Local disk only
dotnet new blazingstoryvrt -n MyBlazorWasmApp1.Stories.VRT \
  --base-url http://localhost:5000
```

The available options are the following.

| Option | Applies to | Description |
| --- | --- | --- |
| `--base-url`, `-b` | always (required) | The URL where the Blazing Story app under test is running. |
| `--storage` | always | `aws`, `azure`, `gcp`, `custom`, or `none` (default: `none`). |
| `--aws-region` | `--storage aws` (required) | The AWS region of the S3 bucket, for example `ap-northeast-1`. |
| `--aws-bucket` | `--storage aws` (required) | The S3 bucket name holding the baseline screenshots. |
| `--az-account` | `--storage azure` (required) | The Azure Storage account name. |
| `--az-container` | `--storage azure` (required) | The blob container name holding the baseline screenshots. |
| `--gcp-bucket` | `--storage gcp` (required) | The Google Cloud Storage bucket name. |
| `--ai-agent`, `-ai` | `--storage custom` | `claude`, `generic`, or `none` (default: `none`). Decides where the storage adapter skill for AI coding agents is placed. |

### What You Get

```
📂 MyBlazorWasmApp1.Stories.VRT
    + 📄 StoryVRT.esproj         👈 lets Visual Studio open the project
    + 📄 package.json            👈 the npm scripts you will run
    + 📄 vrt.config.ts           👈 the app URL and the storage connection values
    + 📄 playwright.config.ts    👈 viewport, diff sensitivity, and other test settings
    + 📄 README.md               👈 the same workflow as this page, tailored to your storage choice
    + 📂 tests
        + 📄 vrt.spec.ts         👈 registers one test per story
    + 📂 scripts                 👈 story index generation and baseline sync
    + 📂 .devcontainer           👈 a container that runs the tests reproducibly
    + 📂 .vscode
```

The generated project also contains a `README.md` written for the storage service you selected, so the exact command sequence for your project is always at hand inside the project folder.

## Setting Up The VRT Project

### Step 1 - Install The npm Packages

Playwright and the storage SDKs are npm packages, so restore them once in the project folder.

```shell
npm install
```

:::note
If you reopen the project in the Dev Container described below, this step runs automatically when the container is created.
:::

### Step 2 - Implement A Custom Storage Adapter (Only For The "Custom" Choice)

Skip this step unless you selected the "Custom" storage service. See [Implementing A Custom Storage Adapter](#implementing-a-custom-storage-adapter) for the details.

### Step 3 - Start The Blazing Story App

The VRT project takes screenshots of a live app, so start the Blazing Story app under test and leave it running.

```shell
dotnet run --project ./MyBlazorWasmApp1.Stories
```

Make sure the URL it listens on is the one you configured as the base URL. If it is not, correct it in `vrt.config.ts`.

## Running The Visual Regression Test

All of the following commands are run in the VRT project folder.

### The First Run: Capture The Baselines

There is no baseline to compare against yet, so capture one.

```shell
npm run snapshots:update
```

Review the captured images under `tests/vrt.spec.ts-snapshots/`, and once they look right, publish them as the team's baselines.

```shell
npm run snapshots:push
```

### Everyday Runs

After changing a component, a stylesheet, or anything else that could affect the appearance, run the test.

```shell
npm test
```

Baselines that are missing on your machine are downloaded from the storage service automatically before the run, so this single command is also all that a fresh clone or a CI agent needs.

If differences are detected, the failing tests are listed in the console. Open the report at `playwright-report/index.html` in a browser to inspect them visually: for each failing story it shows the baseline image, the new screenshot, and the highlighted pixel diff, so you can judge at a glance whether the change was intended. `npm run test:open` runs the test and opens that report for you.

### Accepting An Intentional Change

When a difference is intentional and the new appearance is the correct one from now on, recapture the baselines and publish them again.

```shell
npm run snapshots:update
npm run snapshots:push
```

### The npm Scripts

| Script | What it does |
| --- | --- |
| `npm test` | Runs the visual regression test for every story. |
| `npm run test:open` | Runs the test and then opens `playwright-report/index.html`. |
| `npm run snapshots:update` | Captures screenshots and writes them into `tests/vrt.spec.ts-snapshots/` as the new baselines. |
| `npm run snapshots:push` | Uploads the local baselines to the storage service. |
| `npm run snapshots:pull` | Downloads the baselines from the storage service explicitly. |
| `npm run stories:gen` | Refreshes `tests/stories.json` from the running app without running any test. Useful to make newly added stories appear in an editor's test explorer. |

:::note
`snapshots:push` and `snapshots:pull` exist only when a storage service other than "None" was selected.
:::

### Options Of The Baseline Sync

`snapshots:push` and `snapshots:pull` transfer only the files that are missing or whose content differs, print the resulting plan, and ask for confirmation before doing anything. Options are passed after a `--` separator, for example `npm run snapshots:push -- --dry-run`.

| Option | Description |
| --- | --- |
| `--dry-run` | Shows the plan and transfers nothing. |
| `--yes` | Skips the confirmation prompt, which is what you want on CI. |
| `--force` | Transfers everything, overwriting the destination. |
| `--missing` | Transfers only the files that are absent at the destination. |
| `--filter <glob>` | Restricts the operation to file names matching the glob (`*` and `?` are supported). |
| `--all-platforms` | On pull only. Also downloads the baselines captured on other platforms. |
| `--prune` | Deletes the files at the destination that no longer exist at the source. |
| `--help` | Shows this list. |

:::note
Since baseline file names carry the platform they were captured on, every operation acts on the current platform's set unless you ask otherwise. Pruning is always limited to the current platform, so a push from one platform can never delete another platform's baselines.
:::

## Running In A Dev Container (Recommended)

Screenshots differ subtly between machines. A different graphics stack, a different set of installed fonts, or simply a different operating system is enough to shift a few pixels and produce a failure that means nothing. The cure is to always capture and compare inside the same container.

The generated project therefore ships a Dev Container definition (`.devcontainer/devcontainer.json`) based on the official Playwright container image, with the CLI of your chosen storage service and the Playwright extension for VS Code already included. If you open the VRT project folder in VS Code, run **"Reopen in Container"**, and you are running in the same environment as everybody else, including your CI/CD pipeline.

:::note
When the tests run inside the container, `localhost` in the base URL is automatically rewritten to `host.docker.internal`, so a Blazing Story app running on your host machine is still reachable from inside the container without any configuration change.
:::

## Configuration

### `vrt.config.ts`

Everything you may need to edit is collected in `vrt.config.ts`: the URL of the Blazing Story app under test, and the connection values of the storage service. The file is already filled in with the values you gave when the project was created, so edit it when one of them was wrong or has changed later.

```typescript title="📄 vrt.config.ts (the AWS S3 case)"
export const vrtConfig = {
  storageRegion: process.env.VRT_STORAGE_REGION ?? "ap-northeast-1",
  storageBucket: process.env.VRT_STORAGE_BUCKET ?? "my-vrt-baselines",
  baseURL: process.env.VRT_BASE_URL ?? "http://localhost:5000",
};
```

### Environment Variables And `.env`

As the code above shows, an environment variable takes precedence over the value written in the file. This is how one machine, or one CI job, runs against a different app URL or a different bucket without editing a committed file.

| Environment variable | Overrides |
| --- | --- |
| `VRT_BASE_URL` | The URL of the Blazing Story app under test. |
| `VRT_STORAGE_REGION` | The AWS S3 region. |
| `VRT_STORAGE_BUCKET` | The AWS S3 or Google Cloud Storage bucket name. |
| `VRT_STORAGE_ACCOUNT` | The Azure Storage account name. |
| `VRT_STORAGE_CONTAINER` | The Azure blob container name. |
| `VRT_SKIP_CLOUD` | Set it to `1` to run the tests without touching the storage service, for example when working offline. |

These variables can also be written into a `.env` file at the project root. `vrt.config.ts` loads it automatically, and the generated `.gitignore` keeps it out of your repository, which makes `.env` the right place for anything secret. A variable that is already set in the shell still wins over `.env`.

### `playwright.config.ts`

The test settings live in `playwright.config.ts`. The values worth knowing about are the following.

| Setting | Default | Description |
| --- | --- | --- |
| `expect.toHaveScreenshot.maxDiffPixelRatio` | `0.001` | How much of the image is allowed to differ before the test fails. Raise it if tiny rendering noise makes tests flaky, lower it to catch smaller changes. |
| `use.viewport` | `900` x `600` | The browser viewport the stories are rendered in. |
| `workers` | `4` | How many stories are captured in parallel. |
| `reporter` | HTML | Writes the report into `playwright-report/`. |

Changing the viewport, or anything else that affects rendering, invalidates the existing baselines, so recapture and publish them afterwards.

Everything else follows the standard Playwright Test behavior, so for deeper customization, refer to the [Playwright documentation](https://playwright.dev/docs/test-configuration) and its [snapshot testing guide](https://playwright.dev/docs/test-snapshots).

## Implementing A Custom Storage Adapter

If your baselines belong somewhere other than the three supported cloud services, such as MinIO, WebDAV, Dropbox, or any other S3-compatible storage, create the project with the "Custom" storage service.

That produces the same project, except that `scripts/snapshot-storage.ts` is an empty template. Implement its members, each of which is described by a comment in the file, so that the sync can list, download, upload, and delete the baseline files on your service. Add the connection values your implementation needs to `vrt.config.ts`, following the same pattern as the app URL: read an environment variable first, and fall back to a literal in the file. Everything else in the project is storage agnostic and does not need to be touched.

:::tip
If you selected an AI coding agent when creating the project, you do not have to write the adapter by hand. A **storage adapter skill** is placed in the folder for that agent (`.claude/skills/` for Claude Code, or `.agents/skills/` for GitHub Copilot, OpenAI Codex, Cursor, and others). Tell your agent which service you want to use and ask it to implement the storage adapter, and it will follow that skill to fill in `scripts/snapshot-storage.ts` and add the matching values to `vrt.config.ts`.
:::

:::warning
Secrets such as passwords and access keys must never be written into `vrt.config.ts`, because that file is committed to your repository. Read them from environment variables, and keep them in the git-ignored `.env` file.
:::

## Working With Editors

**VS Code with the [Playwright extension](https://playwright.dev/docs/getting-started-vscode) is the recommended environment.** Every story appears as its own entry in the Test Explorer, so you can run the visual regression test for a single story, watch the diff, and update just that baseline, all from the GUI.

**Visual Studio** can open the project (that is what `StoryVRT.esproj` is for) and can trigger a test run or a baseline update, but the finer-grained operations, and the baseline sync in particular, are done with `npm` commands in a terminal.

## Running On CI/CD

Nothing about the VRT project is specific to a developer machine, but keep the following in mind when you wire it into a pipeline.

- The Blazing Story app under test must be running and reachable from the job, and `VRT_BASE_URL` should point at it.
- The job needs to be authenticated to the storage service in the same way a developer machine is, through that service's CLI or its ambient credentials. The baselines are then downloaded automatically before the tests run.
- Use the same container as your local runs, so that the screenshots being compared were rendered by the same browser, on the same operating system, with the same fonts.
- Publish `playwright-report/` as a build artifact, so a failing run can be inspected visually.

## See Also

- [Blazing Story JavaScript API](./javascript-api), the browser API this VRT project is built on, which you can also use to automate the Blazing Story app in other ways.
