import { describe, expect, it, vi, beforeEach } from 'vitest';
import { getUserProfile, getNotifications, deleteNotification, borrowBook } from './user';
import { ApiError } from '$lib/api';

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
		it('should fetch user profile from correct endpoint', async () => {
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
					headers: expect.objectContaining({
						'X-User-Id': 'test-user-id'
					})
				})
			);
			expect(result).toEqual(mockResponse);
		});

		it('should throw ApiError on non-ok response', async () => {
			mockFetch.mockResolvedValueOnce({
				ok: false,
				status: 401
			});

			try {
				await getUserProfile();
				expect.fail('Should have thrown an error');
			} catch (e) {
				expect(e).toBeInstanceOf(ApiError);
				expect((e as ApiError).status).toBe(401);
			}
		});
	});

	describe('getNotifications', () => {
		it('should fetch notifications from correct endpoint', async () => {
			const mockNotifications = [
				{ id: 'Notifications/1', text: 'Test notification 1' },
				{ id: 'Notifications/2', text: 'Test notification 2' }
			];

			mockFetch.mockResolvedValueOnce({
				ok: true,
				json: async () => mockNotifications
			});

			const result = await getNotifications();

			expect(mockFetch).toHaveBeenCalledWith(
				expect.stringContaining('/api/user/notifications'),
				expect.objectContaining({
					headers: expect.objectContaining({
						'X-User-Id': 'test-user-id'
					})
				})
			);
			expect(result).toEqual(mockNotifications);
		});

		it('should handle notifications with referencedItemId', async () => {
			const mockNotifications = [
				{ id: 'Notifications/1', text: 'Test notification 1', referencedItemId: 'Books/123' },
				{ id: 'Notifications/2', text: 'Test notification 2' }
			];

			mockFetch.mockResolvedValueOnce({
				ok: true,
				json: async () => mockNotifications
			});

			const result = await getNotifications();

			expect(result).toEqual(mockNotifications);
			expect(result[0].referencedItemId).toBe('Books/123');
			expect(result[1].referencedItemId).toBeUndefined();
		});

		it('should throw ApiError on non-ok response', async () => {
			mockFetch.mockResolvedValueOnce({
				ok: false,
				status: 500
			});

			try {
				await getNotifications();
				expect.fail('Should have thrown an error');
			} catch (e) {
				expect(e).toBeInstanceOf(ApiError);
				expect((e as ApiError).status).toBe(500);
			}
		});
	});

	describe('deleteNotification', () => {
		it('should delete notification with correct endpoint and method', async () => {
			const notificationId = 'Notifications/123';

			mockFetch.mockResolvedValueOnce({
				ok: true,
				json: async () => ({})
			});

			await deleteNotification(notificationId);

			expect(mockFetch).toHaveBeenCalledWith(
				expect.stringContaining(`/api/user/notifications/${notificationId}`),
				expect.objectContaining({
					method: 'DELETE',
					headers: expect.objectContaining({
						'X-User-Id': 'test-user-id'
					})
				})
			);
		});

		it('should throw ApiError on non-ok response', async () => {
			mockFetch.mockResolvedValueOnce({
				ok: false,
				status: 403
			});

			try {
				await deleteNotification('Notifications/123');
				expect.fail('Should have thrown an error');
			} catch (e) {
				expect(e).toBeInstanceOf(ApiError);
				expect((e as ApiError).status).toBe(403);
			}
		});
	});

	describe('borrowBook', () => {
		it('should borrow a book with correct endpoint and payload', async () => {
			const mockResponse = {
				id: 'BorrowedBooks/1',
				bookCopyId: 'BookCopies/1',
				bookId: 'Books/123',
				userId: 'Users/test-user-id',
				borrowedFrom: '2025-12-08T10:00:00Z',
				borrowedTo: '2025-12-08T10:30:00Z'
			};

			mockFetch.mockResolvedValueOnce({
				ok: true,
				status: 201,
				json: async () => mockResponse
			});

			const result = await borrowBook('123');

			expect(mockFetch).toHaveBeenCalledWith(
				expect.stringContaining('/api/user/books'),
				expect.objectContaining({
					method: 'POST',
					headers: expect.objectContaining({
						'Content-Type': 'application/json',
						'X-User-Id': 'test-user-id'
					}),
					body: JSON.stringify({ bookId: '123' })
				})
			);
			expect(result).toEqual(mockResponse);
		});

		it('should throw ApiError on concurrency conflict (409)', async () => {
			mockFetch.mockResolvedValueOnce({
				ok: false,
				status: 409
			});

			try {
				await borrowBook('123');
				expect.fail('Should have thrown an error');
			} catch (e) {
				expect(e).toBeInstanceOf(ApiError);
				expect((e as ApiError).status).toBe(409);
			}
		});

		it('should throw ApiError on not found (404)', async () => {
			mockFetch.mockResolvedValueOnce({
				ok: false,
				status: 404
			});

			try {
				await borrowBook('123');
				expect.fail('Should have thrown an error');
			} catch (e) {
				expect(e).toBeInstanceOf(ApiError);
				expect((e as ApiError).status).toBe(404);
			}
		});
	});
});
