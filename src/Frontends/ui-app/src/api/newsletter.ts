import { base } from "./client";

export async function subscribeNewsletter(email: string): Promise<void> {
  const res = await fetch(`${base()}/api/v1/newsletter/subscribe`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email }),
  });
  const data = await res.json().catch(() => ({}));
  if (!res.ok || !(data as { success?: boolean }).success) {
    throw new Error(
      (data as { error?: { message?: string } })?.error?.message ?? "Subscription failed."
    );
  }
}
