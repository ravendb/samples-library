import { describe, expect, it } from 'vitest';
import { apiUrl, API_BASE_URL } from './api';

describe('api.ts', () => {
	describe('apiUrl', () => {
		it('should build URL with base URL and route starting with slash', () => {
			expect(apiUrl('/books', 'http://localhost:5000')).toBe('http://localhost:5000/api/books');
		});

		it('should build URL with base URL and route without leading slash', () => {
			expect(apiUrl('books', 'http://localhost:5000')).toBe('http://localhost:5000/api/books');
		});

		it('should build URL with nested route', () => {
			expect(apiUrl('/books/123', 'http://localhost:5000')).toBe(
				'http://localhost:5000/api/books/123'
			);
		});

		it('should build relative URL when base URL is empty', () => {
			expect(apiUrl('/books', '')).toBe('/api/books');
		});

		it('should handle trailing slash in base URL', () => {
			expect(apiUrl('/books', 'http://localhost:5000/')).toBe('http://localhost:5000/api/books');
		});

		it('should use API_BASE_URL by default', () => {
			// This tests the default behavior using the actual env variable
			// In test environment, VITE_APP_HTTP is empty string
			expect(apiUrl('/books')).toBe(`${API_BASE_URL}/api/books`);
		});
	});

	describe('API_BASE_URL', () => {
		it('should be a string', () => {
			expect(typeof API_BASE_URL).toBe('string');
		});
	});
});
