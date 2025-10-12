import '@testing-library/jest-dom'

// Minimal mocks for Vite env used by api.js
Object.defineProperty(import.meta, 'env', {
  value: { PROD: false },
})

// Mock localStorage for tests
class LocalStorageMock {
  store: Record<string, string> = {}
  getItem(key: string) { return this.store[key] || null }
  setItem(key: string, value: string) { this.store[key] = String(value) }
  removeItem(key: string) { delete this.store[key] }
  clear() { this.store = {} }
}
// @ts-ignore
global.localStorage = new LocalStorageMock()
