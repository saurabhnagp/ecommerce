import { base } from "../api/client";
import { getAccessToken } from "../auth/storage";

function authHeaders(): Record<string, string> {
  const token = getAccessToken();
  return {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

export async function adminFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
  const res = await fetch(`${base()}${path}`, {
    ...options,
    headers: { ...authHeaders(), ...(options.headers as Record<string, string> ?? {}) },
  });
  const data = await res.json().catch(() => ({}));
  if (!res.ok) {
    throw new Error(
      (data as { error?: { message?: string } })?.error?.message ??
        `Request failed (${res.status})`
    );
  }
  return data as T;
}

/* ── Testimonials ── */
export type Testimonial = {
  id: string;
  customerName: string;
  photoUrl?: string;
  comment: string;
  rating: number;
  sortOrder: number;
  isActive?: boolean;
};

export function fetchTestimonials() {
  return adminFetch<{ data: Testimonial[] }>("/api/v1/testimonials");
}

export function createTestimonial(body: Omit<Testimonial, "id">) {
  return adminFetch<{ data: Testimonial }>("/api/v1/testimonials", {
    method: "POST",
    body: JSON.stringify(body),
  });
}

export function updateTestimonial(id: string, body: Partial<Testimonial>) {
  return adminFetch<{ data: Testimonial }>(`/api/v1/testimonials/${id}`, {
    method: "PUT",
    body: JSON.stringify(body),
  });
}

export function deleteTestimonial(id: string) {
  return adminFetch<{ success: boolean }>(`/api/v1/testimonials/${id}`, { method: "DELETE" });
}

/* ── Contact Messages ── */
export type ContactMsg = {
  id: string;
  name: string;
  email: string;
  subject: string;
  comment: string;
  isRead: boolean;
  createdAt: string;
};

export function fetchContactMessages() {
  return adminFetch<{ data: ContactMsg[] }>("/api/v1/contact");
}

export function markContactRead(id: string) {
  return adminFetch<{ success: boolean }>(`/api/v1/contact/${id}/read`, { method: "PATCH" });
}

/* ── Newsletter ── */
export type Subscriber = {
  id: string;
  email: string;
  isActive: boolean;
  subscribedAt: string;
};

export function fetchSubscribers() {
  return adminFetch<{ data: Subscriber[] }>("/api/v1/newsletter/subscribers");
}
