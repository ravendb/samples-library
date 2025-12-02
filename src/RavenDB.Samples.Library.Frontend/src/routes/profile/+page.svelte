<script lang="ts">
	import { onMount } from 'svelte';
	import { getUserId, getUserAvatarUrl } from '$lib/utils/userId';

	let userId = $state('');
	let avatarUrl = $state('');

	onMount(() => {
		userId = getUserId();
		avatarUrl = getUserAvatarUrl(userId);
	});
</script>

<svelte:head>
	<title>Profile | Library</title>
</svelte:head>

<div class="profile-page">
	<h1>Profile</h1>

	<div class="profile-card">
		{#if avatarUrl}
			<img src={avatarUrl} alt="User avatar" class="profile-avatar" />
		{/if}
		<div class="profile-info">
			<p class="profile-label">User ID</p>
			<p class="profile-value">{userId}</p>
		</div>
	</div>
</div>

<style>
	.profile-page {
		max-width: 600px;
		margin: 0 auto;
		padding: 40px 24px;
	}

	h1 {
		margin-bottom: 24px;
		font-size: 28px;
		font-weight: 600;
	}

	.profile-card {
		display: flex;
		align-items: center;
		gap: 24px;
		padding: 24px;
		background: white;
		border: 1px solid #e5e7eb;
		border-radius: 12px;
	}

	.profile-avatar {
		width: 80px;
		height: 80px;
		border-radius: 50%;
		background: #f3f4f6;
	}

	.profile-info {
		display: flex;
		flex-direction: column;
		gap: 4px;
	}

	.profile-label {
		font-size: 14px;
		color: #6b7280;
	}

	.profile-value {
		font-family: monospace;
		font-size: 14px;
		color: #111827;
	}
</style>
