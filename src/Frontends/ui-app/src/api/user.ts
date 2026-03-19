import { base } from "./client";
import { SessionExpiredError } from "./errors";

export type UserProfile = {
  id: string;
  email: string;
  name: string;
  firstName: string;
  lastName: string;
  phone?: string | null;
  gender?: string | null;
  role: string;
  status: string;
  isVerified: boolean;
  createdAt: string;
  lastLoginAt?: string | null;
};

export async function fetchCurrentUser(accessToken: string): Promise<UserProfile> {
  const res = await fetch(`${base()}/api/v1/users/me`, {
    headers: {
      Authorization: `Bearer ${accessToken}`,
      Accept: "application/json",
    },
  });
  if (res.status === 401) {
    throw new SessionExpiredError();
  }
  const data = await res.json().catch(() => ({}));
  if (!res.ok) {
    throw new Error((data as { error?: { message?: string } })?.error?.message ?? "Could not load profile");
  }
  const payload = data as { success?: boolean; data?: UserProfile };
  if (!payload.success || !payload.data) throw new Error("Invalid profile response");
  return payload.data;
}
