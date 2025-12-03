import { callApi } from '$lib/api';

export interface Book {
	id: string;
	title: string;
	author: Author;
}

export interface Author {
	id: string;
	firstName: string;
	lastName: string;
}

/**
 * Fetches a book by its ID from the API.
 * @param id - The book ID (e.g., "123" for "Books/123")
 * @returns The book data
 * @throws Error if the book is not found or API fails
 */
export async function getBookById(id: string): Promise<Book> {
	return callApi<Book>(`/books/${id}`);
}
