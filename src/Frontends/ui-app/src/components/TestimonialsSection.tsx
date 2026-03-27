import { useState, useCallback, useEffect } from "react";
import { fetchActiveTestimonials, type TestimonialApiDto } from "../api/testimonials";
import "./TestimonialsSection.css";

type Testimonial = {
  id: string;
  name: string;
  role: string;
  initials: string;
  photoUrl?: string;
  comment: string;
  rating: number;
};

const VERIFIED_ROLE = "Verified Buyer";

function initialsFromName(name: string): string {
  const parts = name.trim().split(/\s+/).filter(Boolean);
  if (parts.length === 0) return "?";
  if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
}

function mapApiToCard(t: TestimonialApiDto): Testimonial {
  const rating = Number.isFinite(t.rating) ? Math.min(5, Math.max(1, Math.round(t.rating))) : 5;
  return {
    id: t.id,
    name: t.customerName?.trim() || "Customer",
    role: VERIFIED_ROLE,
    initials: initialsFromName(t.customerName ?? ""),
    photoUrl: t.photoUrl?.trim() || undefined,
    comment: t.comment ?? "",
    rating,
  };
}

function Stars({ count }: { count: number }) {
  return (
    <div className="testimonial-card__stars" aria-label={`${count} out of 5 stars`}>
      {Array.from({ length: 5 }, (_, i) => (
        <span key={i}>{i < count ? "\u2605" : "\u2606"}</span>
      ))}
    </div>
  );
}

type Props = {
  /** When set, skips the API and shows these items (e.g. Storybook). */
  testimonials?: Testimonial[];
};

export function TestimonialsSection({ testimonials: testimonialsProp }: Props) {
  const [items, setItems] = useState<Testimonial[] | null>(testimonialsProp ?? null);
  const [loading, setLoading] = useState(!testimonialsProp);
  const [index, setIndex] = useState(0);

  useEffect(() => {
    if (testimonialsProp) {
      setItems(testimonialsProp);
      setLoading(false);
      return;
    }
    let cancelled = false;
    setLoading(true);
    fetchActiveTestimonials()
      .then((rows) => {
        if (!cancelled) {
          setItems(rows.map(mapApiToCard));
          setIndex(0);
        }
      })
      .catch(() => {
        if (!cancelled) setItems([]);
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [testimonialsProp]);

  useEffect(() => {
    setIndex((i) => {
      const total = items?.length ?? 0;
      if (total === 0) return 0;
      return i >= total ? 0 : i;
    });
  }, [items]);

  const testimonials = items ?? [];
  const total = testimonials.length;

  const prev = useCallback(() => setIndex((i) => (i - 1 + total) % total), [total]);
  const next = useCallback(() => setIndex((i) => (i + 1) % total), [total]);

  if (loading) {
    return (
      <section className="testimonials" aria-busy="true">
        <h2 className="testimonials__heading">What Our Clients Say</h2>
        <p className="testimonials__sub">Trusted by thousands of happy customers across India</p>
        <p className="testimonials__loading">Loading testimonials…</p>
        <hr className="testimonials__divider" />
      </section>
    );
  }

  if (total === 0) return null;

  const t = testimonials[index];

  return (
    <section className="testimonials">
      <h2 className="testimonials__heading">What Our Clients Say</h2>
      <p className="testimonials__sub">Trusted by thousands of happy customers across India</p>

      <div className="testimonials__carousel">
        <button
          className="testimonials__arrow testimonials__arrow--prev"
          onClick={prev}
          aria-label="Previous testimonial"
        >
          &#8249;
        </button>

        <div className="testimonial-card">
          {t.photoUrl ? (
            <img className="testimonial-card__avatar" src={t.photoUrl} alt={t.name} />
          ) : (
            <div className="testimonial-card__avatar">{t.initials}</div>
          )}
          <Stars count={t.rating} />
          <p className="testimonial-card__quote">{t.comment}</p>
          <span className="testimonial-card__name">
            {t.name} <span className="testimonial-card__role">&ndash; {t.role}</span>
          </span>
        </div>

        <button
          className="testimonials__arrow testimonials__arrow--next"
          onClick={next}
          aria-label="Next testimonial"
        >
          &#8250;
        </button>
      </div>

      <div className="testimonials__dots">
        {testimonials.map((_, i) => (
          <button
            key={i}
            className={`testimonials__dot${i === index ? " testimonials__dot--active" : ""}`}
            onClick={() => setIndex(i)}
            aria-label={`Go to testimonial ${i + 1}`}
          />
        ))}
      </div>

      <hr className="testimonials__divider" />
    </section>
  );
}
