import { callApi } from '$lib/api';

export interface SearchResult {
	id: string;
	type: 'book' | 'author';
	name: string;
	imageUrl: string;
	link: string;
}

interface ApiSearchResult {
	Id: string;
	Query: string;
}

function transformApiResult(apiResult: ApiSearchResult): SearchResult {
	const id = apiResult.Id;
	const isBook = id.startsWith('Books/');
	const type: 'book' | 'author' = isBook ? 'book' : 'author';
	const seed = encodeURIComponent(id);
	const imageUrl = isBook
		? `https://api.dicebear.com/9.x/shapes/svg?seed=${seed}`
		: `https://api.dicebear.com/9.x/avataaars/svg?seed=${seed}`;

	// Convert "Books/123" to "/books/123" or "Authors/123" to "/authors/123"
	const link = '/' + id.toLowerCase();

	return {
		id,
		type,
		name: apiResult.Query,
		imageUrl,
		link
	};
}

export async function searchBooksAndAuthors(query: string): Promise<SearchResult[]> {
	if (!query.trim()) {
		return [];
	}

	const params = new URLSearchParams({
		query: query
	});

	const apiResults = await callApi<ApiSearchResult[]>(`/search?${params.toString()}`);

	return apiResults.map(transformApiResult);
}
