<script lang="ts">
	import image from '$lib/assets/image.webp';
</script>

<svelte:head>
	<title>Home | Library of Ravens</title>
</svelte:head>

<div class="home-page">
	<div class="hero-section">
		<div class="hero-image-container">
			<img src="{image}" alt="A Raven in a library" class="hero-image" />
		</div>
		<h1 class="hero-title">Library of Ravens</h1>
		<p class="hero-description">
			Welcome to our collection. Discover books, explore knowledge, and find your next great read.
		</p>
	</div>
</div>

<style>
	.home-page {
		max-width: 600px;
		margin: 0 auto;
		padding: 40px 24px;
	}

	.hero-section {
		display: flex;
		flex-direction: column;
		align-items: center;
		text-align: center;
		padding: 24px;
		background: white;
		border: 1px solid #e5e7eb;
		border-radius: 12px;
	}

	.hero-image-container {
		width: 100%;
		max-width: 256px;
		aspect-ratio: 1;
		margin-bottom: 24px;
		border-radius: 12px;
		overflow: hidden;
		background: #f3f4f6;
	}

	.hero-image {
		width: 100%;
		height: 100%;
		object-fit: cover;
	}

	.hero-title {
		margin-bottom: 16px;
		font-size: 28px;
		font-weight: 600;
	}

	.hero-description {
		font-size: 16px;
		color: #6b7280;
		line-height: 1.6;
	}
</style>
