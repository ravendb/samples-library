<script lang="ts">
	import { page } from '$app/state';
	import { resolve } from '$app/paths';
	import { onMount } from 'svelte';
	import { getBookById, type Book } from '$lib/services/book';
	import TipBox from '$lib/components/TipBox.svelte';

	let book = $state<Book | null>(null);
	let loading = $state(true);
	let notFound = $state(false);
	let error = $state<string | null>(null);

	onMount(async () => {
		const id = page.params.id;

		if (id === undefined) {
			notFound = true;
			loading = false;
		}
		else {
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
		}
	});
</script>

<svelte:head>
	<title>{book ? book.title : notFound ? 'Book Not Found' : 'Loading...'} | Library of Ravens</title>
</svelte:head>

<div class="page-container">
	{#if loading}
		<div class="card card-centered loading-state">
			<p>Loading...</p>
		</div>
	{:else if notFound}
		<div class="card card-centered not-found-state">
			<h1>Book Not Found</h1>
			<p>The book you're looking for doesn't exist or has been removed.</p>
			<a href={resolve('/')} class="link-primary">← Back to Home</a>
		</div>
	{:else if error}
		<div class="card card-centered error-state">
			<h1>Error</h1>
			<p>{error}</p>
			<a href={resolve('/')} class="link-primary">← Back to Home</a>
		</div>
	{:else if book}
		<div class="book-content">
			<div class="book-left">
				<div class="card book-card">
					<div class="book-cover">
						<img
							src="https://api.dicebear.com/9.x/shapes/svg?seed={encodeURIComponent(book.id)}"
							alt="Book cover"
							class="image-cover"
						/>
					</div>
					<div class="book-info">
						<h1 class="heading-primary">{book.title}</h1>
						<p class="meta-row">
							<span class="meta-label">ID:</span>
							<span class="meta-value">{book.id}</span>
						</p>
						{#if book.author}
							<p class="meta-meta">
								<span class="meta-label">Author:</span>
								<a
									href={resolve(`/authors/${book.author.id.replace('Authors/', '')}`)}
									class="link-primary">{book.author.firstName} {book.author.lastName}</a
								>
							</p>
						{/if}
					</div>
				</div>
			</div>

			<div class="book-right">
				<TipBox
					contextText="The book details page. Provides information about the book, including the number of copies. You can borrow a copy of this book here if some are available."
					ravendbText="This page uses extensive http caching so that it can minimize the egress data. We leverage ETags for this purpose and the fact, that RavenDB allows you to retrieve them with ease. Additionally, we combine multiple queries using .Include and .Lazy"
				/>
			</div>
		</div>
	{/if}
</div>

<style>
	.book-content {
		display: flex;
		gap: var(--spacing-6);
	}

	.book-left {
		flex: 1;
	}

	.book-right {
		flex: 1;
		display: flex;
		flex-direction: column;
	}

	.book-card {
		display: flex;
		gap: var(--spacing-6);
	}

	.book-cover {
		flex-shrink: 0;
		width: 120px;
		height: 160px;
		background: var(--color-gray-100);
		border-radius: var(--radius-md);
		overflow: hidden;
	}

	.book-info {
		flex: 1;
	}

	@media (max-width: 799px) {
		.book-content {
			flex-direction: column;
		}
	}
</style>
