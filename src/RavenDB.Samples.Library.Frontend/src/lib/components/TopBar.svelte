<script lang="ts">
	import { onMount } from 'svelte';
	import { resolve } from '$app/paths';
	import { getUserId, getUserAvatarUrl } from '$lib/utils/userId';
	import SearchModal from './SearchModal.svelte';

	let userId = $state('');
	let avatarUrl = $state('');
	let searchOpen = $state(false);
	let isMac = $state(false);

	onMount(() => {
		userId = getUserId();
		avatarUrl = getUserAvatarUrl(userId);
		isMac = navigator.platform.toLowerCase().includes('mac');
	});

	function handleGlobalKeydown(e: KeyboardEvent) {
		// Check for Ctrl+K (Windows/Linux) or Cmd+K (Mac)
		if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
			e.preventDefault();
			searchOpen = true;
		}
	}
</script>

<svelte:window onkeydown={handleGlobalKeydown} />

<header class="topbar">
	<div class="topbar-content">
		<div class="topbar-left">
			<a href={resolve('/')} class="logo">
				<span class="logo-text">📚 Library of Ravens</span>
			</a>

			<button
				class="search-trigger"
				onclick={() => (searchOpen = true)}
				aria-label="Open search dialog"
			>
				<svg class="search-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
					<circle cx="11" cy="11" r="8" />
					<path d="M21 21l-4.35-4.35" />
				</svg>
				<span class="search-placeholder">Search...</span>
				<kbd class="kbd">{isMac ? '⌘' : 'Ctrl'}+K</kbd>
			</button>
		</div>

		<nav class="topbar-right">
			<a href={resolve('/profile')} class="user-link">
				{#if avatarUrl}
					<img src={avatarUrl} alt="User avatar" class="user-avatar" />
				{:else}
					<div class="user-avatar-placeholder"></div>
				{/if}
			</a>
		</nav>
	</div>
</header>

<SearchModal bind:isOpen={searchOpen} />

<style>
	.topbar {
		position: sticky;
		top: 0;
		background: white;
		border-bottom: 1px solid #e5e7eb;
		z-index: 50;
	}

	.topbar-content {
		max-width: 1200px;
		margin: 0 auto;
		padding: 12px 24px;
		display: flex;
		align-items: center;
		justify-content: space-between;
	}

	.topbar-left {
		display: flex;
		align-items: center;
		gap: 24px;
	}

	.topbar-right {
		display: flex;
		align-items: center;
	}

	.logo {
		text-decoration: none;
		color: inherit;
	}

	.logo-text {
		font-size: 20px;
		font-weight: 600;
	}

	.search-trigger {
		width: 320px;
		display: flex;
		align-items: center;
		gap: 8px;
		padding: 8px 12px;
		background: #f3f4f6;
		border: 1px solid #e5e7eb;
		border-radius: 8px;
		cursor: pointer;
		transition: all 0.15s;
	}

	.search-trigger:hover {
		border-color: #d1d5db;
		background: #f9fafb;
	}

	.search-icon {
		width: 16px;
		height: 16px;
		stroke-width: 2;
		color: #9ca3af;
	}

	.search-placeholder {
		flex: 1;
		text-align: left;
		color: #9ca3af;
		font-size: 14px;
	}

	.kbd {
		background: white;
		border: 1px solid #e5e7eb;
		border-radius: 4px;
		padding: 2px 6px;
		font-size: 12px;
		color: #6b7280;
		font-family: monospace;
	}

	.user-link {
		display: block;
		transition: opacity 0.15s;
	}

	.user-link:hover {
		opacity: 0.8;
	}

	.user-avatar {
		width: 36px;
		height: 36px;
		border-radius: 50%;
		background: #f3f4f6;
	}

	.user-avatar-placeholder {
		width: 36px;
		height: 36px;
		border-radius: 50%;
		background: #e5e7eb;
	}
</style>
