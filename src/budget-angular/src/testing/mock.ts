import { vi } from 'vitest';

/**
 * A utility type that maps all function properties of T to vi mock functions.
 */
export type Mocked<T> = {
  [K in keyof T]: T[K] extends (...args: any[]) => any ? ReturnType<typeof vi.fn<T[K]>> : T[K];
};

/**
 * So this is a simple mock function that creates a Proxy object. When a property is accessed on the Proxy,
 * it checks if that property already exists in the internal store. If it does, it returns the existing value.
 * If not, it creates a new stub function using `vi.fn()`, stores it in the internal store, and then returns it.
 *
 * This allows for dynamic creation of mock functions on-the-fly, making it easy to mock entire services or objects
 * @returns A Proxy instance, typed to T
 */
export function mock<T extends object>(): Mocked<T> {
  const store: Record<string | symbol, any> = {};

  return new Proxy(store, {
    get(target, prop) {
      // If property already exists → return it
      if (prop in target) return target[prop];

      // Otherwise create a stub function
      const fn = vi.fn();
      target[prop] = fn;
      return fn;
    },

    set(target, prop, value) {
      target[prop] = value;
      return true;
    },
  }) as Mocked<T>;
}
