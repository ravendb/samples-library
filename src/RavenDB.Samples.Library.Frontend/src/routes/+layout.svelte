<script lang="ts">
	import '../app.css';
	import fav16 from '$lib/assets/favicon-16x16.png';
	import fav32 from '$lib/assets/favicon-32x32.png';
	import favApple from '$lib/assets/apple-touch-icon.png';
	import manifest from '$lib/assets/site.webmanifest';
	import TopBar from '$lib/components/TopBar.svelte';

	let { children } = $props();
</script>

<svelte:head>
	<link rel="apple-touch-icon" sizes="180x180" href="{favApple}">
	<link rel="icon" type="image/png" sizes="32x32" href="{fav32}">
	<link rel="icon" type="image/png" sizes="16x16" href="{fav16}">
	<link rel="manifest" href="{manifest}">
</svelte:head>

<TopBar />
<main>
	{@render children()}
</main>

<style>
	:global(*) {
		margin: 0;
		padding: 0;
		box-sizing: border-box;
	}

	:global(body) {
		font-family:
			-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Open Sans',
			'Helvetica Neue', sans-serif;
		background: var(--color-gray-50);
		color: var(--color-gray-900);
		line-height: 1.5;
	}

	main {
		min-height: calc(100vh - 61px);
	}
</style>
