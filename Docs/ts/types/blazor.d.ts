export interface DotNetObjectReference {
    invokeMethod<T>(methodName: string, ...args: unknown[]): T;
    invokeMethodAsync<T>(methodName: string, ...args: unknown[]): Promise<T>;
}