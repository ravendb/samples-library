<script lang="ts">
	import image from '$lib/assets/image.webp';
</script>

<svelte:head>
	<title>Home | Library of Ravens</title>
</svelte:head>

<div class="page-container">
	<div class="card card-centered hero-section">
		<div class="hero-image-container">
			<img src="{image}" alt="A Raven in a library" class="image-cover" />
		</div>
		<h1 class="hero-title">Library of Ravens</h1>
		<p class="hero-description">
			Welcome to our collection. Discover books, explore knowledge, and find your next great read.
		</p>
	</div>
</div>

<style>
	.hero-section {
		display: flex;
		flex-direction: column;
		align-items: center;
	}

	.hero-image-container {
		width: 100%;
		max-width: 256px;
		aspect-ratio: 1;
		margin-bottom: var(--spacing-6);
		border-radius: var(--radius-lg);
		overflow: hidden;
		background: var(--color-gray-100);
	}

	.hero-title {
		margin-bottom: var(--spacing-4);
		font-size: var(--font-size-2xl);
		font-weight: 600;
	}

	.hero-description {
		font-size: var(--font-size-base);
		color: var(--color-gray-500);
		line-height: 1.6;
	}
</style>
