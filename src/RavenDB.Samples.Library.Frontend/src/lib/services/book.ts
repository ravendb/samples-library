import { callApi } from '$lib/api';

export interface Book {
	id: string;
	title: string;
	author: Author;
	availability?: {
		available: number;
		total: number;
	};
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

/**
 * Fetches random books for the home page from the API.
 * @returns An array of books with their authors
 * @throws Error if the API fails
 */
export async function getHomeBooks(): Promise<Book[]> {
	return callApi<Book[]>(`/home/books`);
}

export interface BorrowResponse {
	success: boolean;
	bookId: string;
	borrowedFrom: string;
	borrowedTo: string;
}

/**
 * Borrows a book for the current user.
 * @param id - The book ID (e.g., "123" for "Books/123")
 * @returns The borrow response with borrow dates
 * @throws Error with status 403 if there's a concurrency error (book already borrowed)
 * @throws Error with status 404 if no copies are available
 */
export async function borrowBook(id: string): Promise<BorrowResponse> {
	return callApi<BorrowResponse>(`/user/books/borrow/${id}`, {
		method: 'POST'
	});
}
