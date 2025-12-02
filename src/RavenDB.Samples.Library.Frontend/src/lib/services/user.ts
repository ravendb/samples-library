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

/**
 * Fetches the user profile from the API.
 * @returns The user profile including borrowed books
 */
export async function getUserProfile(): Promise<UserProfile> {
	return callApi<UserProfile>('/user/profile');
}
