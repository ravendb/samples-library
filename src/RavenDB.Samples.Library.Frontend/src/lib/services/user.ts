import { callApi } from '$lib/api';

export interface BorrowedBook {
	id: string;
	title: string;
	overdue: boolean;
}

export interface UserProfile {
	id: string;
	borrowed: BorrowedBook[];
}

export interface Notification {
	id: string;
	text: string;
	referencedItemId?: string;
}

export interface BorrowBookResponse {
	id: string;
	bookCopyId: string;
	bookId: string;
	userId: string;
	borrowedFrom: string;
	borrowedTo: string;
}

/**
 * Fetches the user profile from the API.
 * @returns The user profile including borrowed books
 */
export async function getUserProfile(): Promise<UserProfile> {
	return callApi<UserProfile>('/user/profile');
}

/**
 * Fetches the user's notifications from the API.
 * @returns An array of notifications
 */
export async function getNotifications(): Promise<Notification[]> {
	return callApi<Notification[]>('/user/notifications');
}

/**
 * Deletes a notification by its ID.
 * @param id - The notification ID to delete
 */
export async function deleteNotification(id: string): Promise<void> {
	// Normalize the identifier first
	id = id.replace('Notifications/', '');

	await callApi<void>(`/user/notifications/${id}`, {
		method: 'DELETE'
	});
}

/**
 * Returns a borrowed book by its ID.
 * @param id - The UserBook ID to return
 */
export async function returnBook(id: string): Promise<void> {
	// Normalize the identifier first
	id = id.replace('UserBooks/', '');

	await callApi<void>(`/user/return/${id}`, {
		method: 'POST'
	});
}

/**
 * Borrows a book by its ID.
 * @param id - The book ID to borrow (with or without 'Books/' prefix)
 * @returns The borrowed book details including borrowedFrom and borrowedTo
 * @throws Error with status 409 on concurrency conflict, or other status codes for other errors
 */
export async function borrowBook(id: string): Promise<BorrowBookResponse> {
	// Normalize the identifier first
	const bookId = id.replace('Books/', '');

	return callApi<BorrowBookResponse>('/user/books', {
		method: 'POST',
		headers: {
			'Content-Type': 'application/json'
		},
		body: JSON.stringify({ bookId })
	});
}
