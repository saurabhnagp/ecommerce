import { base } from "./client";

export type ContactMessagePayload = {
  name: string;
  email: string;
  subject: string;
  comment: string;
};

export async function submitContactMessage(payload: ContactMessagePayload): Promise<void> {
  const res = await fetch(`${base()}/api/v1/contact`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
  const data = await res.json().catch(() => ({}));
  if (!res.ok || !(data as { success?: boolean }).success) {
    throw new Error(
      (data as { error?: { message?: string } })?.error?.message ?? "Failed to send message."
    );
  }
}
