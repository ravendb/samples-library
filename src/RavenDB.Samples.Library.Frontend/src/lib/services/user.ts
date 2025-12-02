import { apiUrl } from '$lib/api';
import { getUserId } from '$lib/utils/userId';

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
	const userId = getUserId();

	const response = await fetch(apiUrl('/user/profile'), {
		headers: {
			'X-User-Id': userId
		}
	});

	if (!response.ok) {
		throw new Error(`Failed to fetch user profile: ${response.status}`);
	}

	return response.json();
}
