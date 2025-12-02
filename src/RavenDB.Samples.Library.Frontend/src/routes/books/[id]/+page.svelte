<script lang="ts">
	import { page } from '$app/state';
	import { resolve } from '$app/paths';
	import { onMount } from 'svelte';
	import { getBookById, type Book } from '$lib/services/book';

	let book = $state<Book | null>(null);
	let loading = $state(true);
	let notFound = $state(false);
	let error = $state<string | null>(null);

	onMount(async () => {
		const id = page.params.id;

		try {
			book = await getBookById(id);
		} catch (e) {
			if (e instanceof Error && e.message.includes('404')) {
				notFound = true;
			} else {
				error = e instanceof Error ? e.message : 'Failed to load book';
			}
		} finally {
			loading = false;
		}
	});
</script>

<svelte:head>
	<title>{book ? book.title : notFound ? 'Book Not Found' : 'Loading...'} | Library of Ravens</title
	>
</svelte:head>

<div class="book-page">
	{#if loading}
		<div class="loading-card">
			<p>Loading...</p>
		</div>
	{:else if notFound}
		<div class="not-found-card">
			<h1>Book Not Found</h1>
			<p>The book you're looking for doesn't exist or has been removed.</p>
			<a href={resolve('/')} class="back-link">← Back to Home</a>
		</div>
	{:else if error}
		<div class="error-card">
			<h1>Error</h1>
			<p>{error}</p>
			<a href={resolve('/')} class="back-link">← Back to Home</a>
		</div>
	{:else if book}
		<div class="book-card">
			<div class="book-cover">
				<img
					src="https://api.dicebear.com/9.x/shapes/svg?seed={encodeURIComponent(book.id)}"
					alt="Book cover"
					class="cover-image"
				/>
			</div>
			<div class="book-info">
				<h1>{book.title}</h1>
				<p class="book-meta">
					<span class="label">ID:</span>
					<span class="value">{book.id}</span>
				</p>
				{#if book.authorId}
					<p class="book-meta">
						<span class="label">Author:</span>
						<a
							href={resolve(`/authors/${book.authorId.replace('Authors/', '')}`)}
							class="author-link">{book.authorId}</a
						>
					</p>
				{/if}
			</div>
		</div>
	{/if}
</div>

<style>
	.book-page {
		max-width: 600px;
		margin: 0 auto;
		padding: 40px 24px;
	}

	.loading-card,
	.not-found-card,
	.error-card,
	.book-card {
		padding: 24px;
		background: white;
		border: 1px solid #e5e7eb;
		border-radius: 12px;
	}

	.loading-card {
		text-align: center;
		color: #6b7280;
	}

	.not-found-card,
	.error-card {
		text-align: center;
	}

	.not-found-card h1,
	.error-card h1 {
		margin-bottom: 12px;
		font-size: 24px;
		font-weight: 600;
		color: #111827;
	}

	.not-found-card p,
	.error-card p {
		margin-bottom: 24px;
		color: #6b7280;
	}

	.error-card p {
		color: #dc2626;
	}

	.back-link {
		display: inline-block;
		color: #2563eb;
		text-decoration: none;
	}

	.back-link:hover {
		text-decoration: underline;
	}

	.book-card {
		display: flex;
		gap: 24px;
	}

	.book-cover {
		flex-shrink: 0;
		width: 120px;
		height: 160px;
		background: #f3f4f6;
		border-radius: 8px;
		overflow: hidden;
	}

	.cover-image {
		width: 100%;
		height: 100%;
		object-fit: cover;
	}

	.book-info {
		flex: 1;
	}

	.book-info h1 {
		margin-bottom: 16px;
		font-size: 24px;
		font-weight: 600;
		color: #111827;
	}

	.book-meta {
		margin-bottom: 8px;
		font-size: 14px;
	}

	.label {
		color: #6b7280;
		margin-right: 8px;
	}

	.value {
		font-family: monospace;
		color: #111827;
	}

	.author-link {
		color: #2563eb;
		text-decoration: none;
	}

	.author-link:hover {
		text-decoration: underline;
	}
</style>
