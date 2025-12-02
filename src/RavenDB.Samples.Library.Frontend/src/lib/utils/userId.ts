const USER_ID_KEY = 'library_user_id';

export function getUserId(): string {
	if (typeof localStorage === 'undefined') {
		return generateUUID();
	}

	let userId = localStorage.getItem(USER_ID_KEY);
	if (!userId) {
		userId = generateUUID();
		localStorage.setItem(USER_ID_KEY, userId);
	}
	return userId;
}

function generateUUID(): string {
	return crypto.randomUUID();
}

export function getUserAvatarUrl(userId: string): string {
	return `https://api.dicebear.com/9.x/fun-emoji/svg?seed=${encodeURIComponent(userId)}`;
}
