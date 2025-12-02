import { describe, expect, it, vi, beforeEach } from 'vitest';
import { getUserProfile } from './user';

// Mock fetch globally
const mockFetch = vi.fn();
global.fetch = mockFetch;

// Mock localStorage
const mockStorage: Record<string, string> = {};
vi.stubGlobal('localStorage', {
	getItem: (key: string) => mockStorage[key] ?? null,
	setItem: (key: string, value: string) => {
		mockStorage[key] = value;
	}
});

// Mock crypto.randomUUID
vi.stubGlobal('crypto', {
	randomUUID: () => 'test-user-id'
});

describe('user service', () => {
	beforeEach(() => {
		vi.clearAllMocks();
		Object.keys(mockStorage).forEach((key) => delete mockStorage[key]);
	});

	describe('getUserProfile', () => {
		it('should fetch user profile with correct headers', async () => {
			const mockResponse = {
				id: 'Users/test-user-id',
				borrowed: [{ id: 'book-1', title: 'Test Book', authorId: 'author-1' }]
			};

			mockFetch.mockResolvedValueOnce({
				ok: true,
				json: async () => mockResponse
			});

			const result = await getUserProfile();

			expect(mockFetch).toHaveBeenCalledWith(
				expect.stringContaining('/api/user/profile'),
				expect.objectContaining({
					headers: {
						'X-User-Id': 'test-user-id'
					}
				})
			);
			expect(result).toEqual(mockResponse);
		});

		it('should throw error on non-ok response', async () => {
			mockFetch.mockResolvedValueOnce({
				ok: false,
				status: 401
			});

			await expect(getUserProfile()).rejects.toThrow('Failed to fetch user profile: 401');
		});
	});
});
