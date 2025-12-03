<script lang="ts">
	import { page } from '$app/state';
	import { resolve } from '$app/paths';
	import { onMount } from 'svelte';
	import { getAuthorById, type Author } from '$lib/services/author';

	let author = $state<Author | null>(null);
	let loading = $state(true);
	let notFound = $state(false);
	let error = $state<string | null>(null);

	onMount(async () => {
		const id = page.params.id;

		if (!id) {
			notFound = true;
			loading = false;
			return;
		}

		try {
			author = await getAuthorById(id);
		} catch (e) {
			if (e instanceof Error && e.message.includes('404')) {
				notFound = true;
			} else {
				error = e instanceof Error ? e.message : 'Failed to load author';
			}
		} finally {
			loading = false;
		}
	});
</script>

<svelte:head>
	<title
		>{author
			? `${author.firstName} ${author.lastName}`
			: notFound
				? 'Author Not Found'
				: 'Loading...'} | Library of Ravens</title
	>
</svelte:head>

<div class="page-container">
	{#if loading}
		<div class="card card-centered loading-state">
			<p>Loading...</p>
		</div>
	{:else if notFound}
		<div class="card card-centered not-found-state">
			<h1>Author Not Found</h1>
			<p>The author you're looking for doesn't exist or has been removed.</p>
			<a href={resolve('/')} class="link-primary">← Back to Home</a>
		</div>
	{:else if error}
		<div class="card card-centered error-state">
			<h1>Error</h1>
			<p>{error}</p>
			<a href={resolve('/')} class="link-primary">← Back to Home</a>
		</div>
	{:else if author}
		<div class="card author-card">
			<div class="author-avatar avatar-round">
				<img
					src="https://api.dicebear.com/9.x/avataaars/svg?seed={encodeURIComponent(author.id)}"
					alt="Author avatar"
					class="image-cover"
				/>
			</div>
			<div class="author-info">
				<h1 class="heading-primary">{author.firstName} {author.lastName}</h1>
				<p class="meta-row">
					<span class="meta-label">ID:</span>
					<span class="meta-value">{author.id}</span>
				</p>
			</div>
		</div>

		{#if author.books && author.books.length > 0}
			<div class="card books-section">
				<h2 class="heading-section">Books</h2>
				<div class="books-list">
					{#each author.books as book (book.id)}
						<a href={resolve(`/books/${book.id.replace('Books/', '')}`)} class="book-item">
							<div class="book-cover-small">
								<img
									src="https://api.dicebear.com/9.x/shapes/svg?seed={encodeURIComponent(book.id)}"
									alt="Book cover for {book.title}"
									class="image-cover"
								/>
							</div>
							<div class="book-item-info">
								<h3 class="book-title">{book.title}</h3>
							</div>
						</a>
					{/each}
				</div>
			</div>
		{/if}
	{/if}
</div>

<style>
	.author-card {
		display: flex;
		gap: var(--spacing-6);
		align-items: center;
	}

	.author-avatar {
		flex-shrink: 0;
		width: 120px;
		height: 120px;
	}

	.author-info {
		flex: 1;
	}

	.books-section {
		margin-top: var(--spacing-6);
	}

	.books-list {
		display: grid;
		grid-template-columns: repeat(2, 1fr);
		gap: var(--spacing-3);
	}

	.book-item {
		display: flex;
		flex-direction: column;
		gap: var(--spacing-2);
		padding: var(--spacing-3);
		border: 1px solid var(--color-gray-200);
		border-radius: var(--radius-md);
		text-decoration: none;
		color: inherit;
		transition: background-color 0.2s, border-color 0.2s;
	}

	.book-item:hover {
		background-color: var(--color-gray-50);
		border-color: var(--color-blue-600);
	}

	.book-cover-small {
		width: 100%;
		aspect-ratio: 3 / 4;
		background: var(--color-gray-100);
		border-radius: var(--radius-sm);
		overflow: hidden;
	}

	.book-item-info {
		flex: 1;
	}

	.book-title {
		font-size: var(--font-size-sm);
		font-weight: 500;
		color: var(--color-gray-900);
		margin: 0;
		line-height: 1.4;
	}

	@media (max-width: 480px) {
		.books-list {
			grid-template-columns: 1fr;
		}
	}
</style>
