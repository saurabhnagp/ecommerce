import { base } from "./client";
import { SessionExpiredError } from "./errors";

export type TokenResponse = {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
};

export type UserSummary = {
  id: string;
  email: string;
  name: string;
  phone?: string;
  role?: string;
};

async function parseJson<T>(res: Response): Promise<T> {
  const text = await res.text();
  if (!text) return {} as T;
  try {
    return JSON.parse(text) as T;
  } catch {
    throw new Error("Invalid response from server");
  }
}

export async function login(email: string, password: string, rememberMe: boolean) {
  const res = await fetch(`${base()}/api/v1/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password, rememberMe }),
  });
  const data = await parseJson<{
    success: boolean;
    data?: { user: UserSummary; tokens: TokenResponse };
    error?: { code: string; message: string };
  }>(res);
  if (!res.ok || !data.success || !data.data) {
    throw new Error(data.error?.message ?? "Login failed");
  }
  return data.data;
}

export async function register(payload: {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
  name: string;
  phone?: string;
  gender?: string;
}) {
  const res = await fetch(`${base()}/api/v1/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
  const data = await parseJson<{
    success: boolean;
    data?: { user: UserSummary; tokens: TokenResponse };
    message?: string;
    error?: { code: string; message: string };
  }>(res);
  if (!res.ok || !data.success) {
    throw new Error(data.error?.message ?? "Registration failed");
  }
  return data;
}

export async function forgotPassword(email: string) {
  const res = await fetch(`${base()}/api/v1/auth/forgot-password`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email }),
  });
  const data = await parseJson<{ success: boolean; message?: string }>(res);
  if (!res.ok) throw new Error("Request failed");
  return data.message ?? "If an account exists, check your email.";
}

export async function resetPassword(token: string, password: string, confirmPassword: string) {
  const res = await fetch(`${base()}/api/v1/auth/reset-password`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ token, password, confirmPassword }),
  });
  const data = await parseJson<{ success: boolean; error?: { message: string } }>(res);
  if (!res.ok || !data.success) {
    throw new Error(data.error?.message ?? "Reset failed");
  }
}

export async function changePassword(
  currentPassword: string,
  newPassword: string,
  confirmNewPassword: string,
  accessToken: string
) {
  const res = await fetch(`${base()}/api/v1/auth/change-password`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${accessToken}`,
    },
    body: JSON.stringify({ currentPassword, newPassword, confirmNewPassword }),
  });
  if (res.status === 401) {
    throw new SessionExpiredError();
  }
  const data = await parseJson<{ success: boolean; error?: { message: string } }>(res);
  if (!res.ok || !data.success) {
    throw new Error(data.error?.message ?? "Could not change password");
  }
}
