import { callApi } from '$lib/api';

export interface Book {
	id: string;
	title: string;
	authorId: string;
}

export interface UserProfile {
	id: string;
	borrowed: Book[];
}

export interface Notification {
	id: string;
	text: string;
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
	await callApi<void>(`/user/notifications/${id}`, {
		method: 'DELETE'
	});
}
