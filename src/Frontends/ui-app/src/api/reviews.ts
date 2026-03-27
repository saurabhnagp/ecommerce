import { getAccessToken } from "../auth/storage";
import { productServiceBase } from "./products";
import type { PagedResult } from "./products";

export type ProductReviewDto = {
  id: string;
  productId: string;
  userId: string;
  rating: number;
  title?: string | null;
  comment?: string | null;
  isVerifiedPurchase: boolean;
  isApproved: boolean;
  helpfulCount: number;
  notHelpfulCount: number;
  reviewerDisplayName?: string | null;
  reviewerPhotoUrl?: string | null;
  /** Current user's vote when request was authenticated: like | dislike */
  myVote?: string | null;
  createdAt: string;
};

export type CreateProductReviewBody = {
  rating: number;
  title?: string;
  comment?: string;
  isVerifiedPurchase?: boolean;
  reviewerPhotoUrl?: string;
};

async function parseJson<T>(res: Response): Promise<T> {
  const json = await res.json().catch(() => ({}));
  if (!res.ok) {
    throw new Error(
      (json as { error?: { message?: string } })?.error?.message ??
        `Request failed (${res.status})`
    );
  }
  return json as T;
}

function normalizePagedResult<T>(data: unknown): PagedResult<T> {
  if (data == null || typeof data !== "object") throw new Error("Invalid paged result shape");
  const d = data as Record<string, unknown>;
  const raw = d.items ?? d.Items;
  if (!Array.isArray(raw)) throw new Error("Invalid paged result: items");
  const totalCount = Number(d.totalCount ?? d.TotalCount ?? 0);
  const page = Number(d.page ?? d.Page ?? 1);
  const pageSize = Number(d.pageSize ?? d.PageSize ?? raw.length);
  const totalPages = Number(d.totalPages ?? d.TotalPages ?? 0);
  return {
    items: raw as T[],
    totalCount: Number.isFinite(totalCount) ? totalCount : 0,
    page: Number.isFinite(page) ? page : 1,
    pageSize: Number.isFinite(pageSize) ? pageSize : raw.length,
    totalPages: Number.isFinite(totalPages) ? totalPages : 0,
  };
}

export async function fetchProductReviews(
  productId: string,
  page = 1,
  pageSize = 10,
  token?: string | null
): Promise<PagedResult<ProductReviewDto>> {
  const root = productServiceBase();
  const headers: Record<string, string> = { Accept: "application/json" };
  const t = token ?? getAccessToken();
  if (t) headers.Authorization = `Bearer ${t}`;
  const res = await fetch(
    `${root}/v1/products/${encodeURIComponent(productId)}/reviews?page=${page}&pageSize=${pageSize}`,
    { headers }
  );
  const body = await parseJson<{ success?: boolean; data?: unknown }>(res);
  if (!body.success || body.data == null) throw new Error("Invalid reviews response");
  return normalizePagedResult<ProductReviewDto>(body.data);
}

export async function createProductReview(
  productId: string,
  body: CreateProductReviewBody,
  token: string
): Promise<ProductReviewDto> {
  const root = productServiceBase();
  const res = await fetch(`${root}/v1/products/${encodeURIComponent(productId)}/reviews`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Accept: "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({
      rating: body.rating,
      title: body.title?.trim() || undefined,
      comment: body.comment?.trim() || undefined,
      isVerifiedPurchase: body.isVerifiedPurchase ?? false,
      reviewerPhotoUrl: body.reviewerPhotoUrl?.trim() || undefined,
    }),
  });
  const json = await parseJson<{ success?: boolean; data?: ProductReviewDto }>(res);
  if (!json.success || !json.data) throw new Error("Invalid create review response");
  return json.data;
}

export async function voteOnProductReview(
  productId: string,
  reviewId: string,
  action: "like" | "dislike",
  token: string
): Promise<ProductReviewDto> {
  const root = productServiceBase();
  const res = await fetch(
    `${root}/v1/products/${encodeURIComponent(productId)}/reviews/${encodeURIComponent(reviewId)}/vote`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Accept: "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({ action }),
    }
  );
  const json = await parseJson<{ success?: boolean; data?: ProductReviewDto }>(res);
  if (!json.success || !json.data) throw new Error("Invalid vote response");
  return json.data;
}
