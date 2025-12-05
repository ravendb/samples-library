import { callApi } from '$lib/api';
import { idToLink } from '$lib/utils/links';

export interface SearchResult {
	id: string;
	type: string;
	name: string;
	imageUrl: string;
	link: string;
}

interface ApiSearchResult {
	id: string;
	query: string;
}

function transformApiResult(result: ApiSearchResult): SearchResult {
	const id = result.id;
	const seed = encodeURIComponent(id);
	const prefix = id.slice(0, id.indexOf('/'));
	let type: string = '';
	let imageUrl: string = '';

	switch (prefix) {
		case 'Books':
			type = 'book';
			imageUrl = `https://api.dicebear.com/9.x/shapes/svg?seed=${seed}`;
			break;
		case 'Authors':
			type = 'author';
			imageUrl = `https://api.dicebear.com/9.x/avataaars/svg?seed=${seed}`;
			break;
	}

	// Convert "Books/123" to "/books/123" or "Authors/123" to "/authors/123"
	const link = idToLink(id) ?? '';

	return {
		id,
		type,
		name: result.query,
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
