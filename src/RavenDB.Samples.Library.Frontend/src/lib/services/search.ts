export interface SearchResult {
	id: string;
	type: 'book' | 'author';
	name: string;
	imageUrl: string;
	link: string;
}

export async function searchBooksAndAuthors(query: string): Promise<SearchResult[]> {
	// Mock implementation - replace with actual API call
	if (!query.trim()) {
		return [];
	}

	// Simulate network delay
	await new Promise((resolve) => setTimeout(resolve, 300));

	const mockBooks: SearchResult[] = [
		{
			id: 'book-1',
			type: 'book',
			name: 'The Great Gatsby',
			imageUrl: 'https://api.dicebear.com/9.x/shapes/svg?seed=book1',
			link: '/books/book-1'
		},
		{
			id: 'book-2',
			type: 'book',
			name: 'To Kill a Mockingbird',
			imageUrl: 'https://api.dicebear.com/9.x/shapes/svg?seed=book2',
			link: '/books/book-2'
		},
		{
			id: 'book-3',
			type: 'book',
			name: '1984',
			imageUrl: 'https://api.dicebear.com/9.x/shapes/svg?seed=book3',
			link: '/books/book-3'
		}
	];

	const mockAuthors: SearchResult[] = [
		{
			id: 'author-1',
			type: 'author',
			name: 'F. Scott Fitzgerald',
			imageUrl: 'https://api.dicebear.com/9.x/avataaars/svg?seed=author1',
			link: '/authors/author-1'
		},
		{
			id: 'author-2',
			type: 'author',
			name: 'Harper Lee',
			imageUrl: 'https://api.dicebear.com/9.x/avataaars/svg?seed=author2',
			link: '/authors/author-2'
		},
		{
			id: 'author-3',
			type: 'author',
			name: 'George Orwell',
			imageUrl: 'https://api.dicebear.com/9.x/avataaars/svg?seed=author3',
			link: '/authors/author-3'
		}
	];

	const allResults = [...mockBooks, ...mockAuthors];
	const lowerQuery = query.toLowerCase();

	return allResults.filter((item) => item.name.toLowerCase().includes(lowerQuery));
}
