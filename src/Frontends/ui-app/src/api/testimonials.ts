import { base } from "./client";

/** Active storefront testimonials (UserService, no auth). */
export type TestimonialApiDto = {
  id: string;
  customerName: string;
  photoUrl?: string | null;
  comment: string;
  rating: number;
  sortOrder: number;
};

export async function fetchActiveTestimonials(): Promise<TestimonialApiDto[]> {
  const root = base();
  const res = await fetch(`${root}/api/v1/testimonials`, {
    headers: { Accept: "application/json" },
  });
  const json = await res.json().catch(() => ({}));
  if (!res.ok) {
    throw new Error(
      (json as { error?: { message?: string } })?.error?.message ??
        `Request failed (${res.status})`
    );
  }
  const body = json as { success?: boolean; data?: TestimonialApiDto[] };
  if (!body.success || !Array.isArray(body.data)) throw new Error("Invalid testimonials response");
  return body.data;
}
