import { useState, useCallback } from "react";
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

const SAMPLE_TESTIMONIALS: Testimonial[] = [
  {
    id: "1",
    name: "Ananya Gupta",
    role: "Verified Buyer",
    initials: "AG",
    comment:
      "Absolutely love the quality of clothing I received! The fabric is premium, fits perfectly, and delivery was super fast. AmCart has become my go-to for online shopping.",
    rating: 5,
  },
  {
    id: "2",
    name: "Vikram Singh",
    role: "Verified Buyer",
    initials: "VS",
    comment:
      "Great selection of men's formal wear. I ordered two blazers and a pair of trousers — all arrived within 3 days. The return process is hassle-free too. Highly recommended!",
    rating: 5,
  },
  {
    id: "3",
    name: "Sneha Reddy",
    role: "Verified Buyer",
    initials: "SR",
    comment:
      "The ethnic wear collection is stunning. I bought a lehenga for my sister's wedding and received so many compliments. The prices are very reasonable for the quality you get.",
    rating: 4,
  },
  {
    id: "4",
    name: "Arjun Mehta",
    role: "Verified Buyer",
    initials: "AM",
    comment:
      "I've been shopping here for over a year now. The customer service is excellent, and I love how they keep adding new styles every week. Five stars without a doubt!",
    rating: 5,
  },
  {
    id: "5",
    name: "Pooja Iyer",
    role: "Verified Buyer",
    initials: "PI",
    comment:
      "Ordered a set of kurtis and they look even better in person than the photos. The size guide is accurate and packaging was very neat. Will definitely order again!",
    rating: 4,
  },
];

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
  testimonials?: Testimonial[];
};

export function TestimonialsSection({ testimonials = SAMPLE_TESTIMONIALS }: Props) {
  const [index, setIndex] = useState(0);
  const total = testimonials.length;

  const prev = useCallback(() => setIndex((i) => (i - 1 + total) % total), [total]);
  const next = useCallback(() => setIndex((i) => (i + 1) % total), [total]);

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
