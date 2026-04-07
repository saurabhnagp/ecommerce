import { useState } from "react";
import { submitContactMessage } from "../api/contact";
import "./ContactPage.css";

const CONTACT_PERSONS = [
  { name: "Rajesh Sharma", role: "Director", phone: "+91 98765 43210", email: "rajesh@amcart.com", initials: "RS" },
  { name: "Priya Verma", role: "Manager", phone: "+91 87654 32109", email: "priya@amcart.com", initials: "PV" },
  { name: "Amit Patel", role: "Assistant Manager", phone: "+91 76543 21098", email: "amit@amcart.com", initials: "AP" },
];

export function ContactPage() {
  const [form, setForm] = useState({ name: "", email: "", subject: "", comment: "" });
  const [submitting, setSubmitting] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);

  function update(field: string) {
    return (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
      setForm((f) => ({ ...f, [field]: e.target.value }));
      setSuccess(false);
      setError(null);
    };
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await submitContactMessage(form);
      setSuccess(true);
      setForm({ name: "", email: "", subject: "", comment: "" });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to send message. Please try again.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="contact-page">
      {/* ——— Left Column ——— */}
      <div>
        <iframe
          className="contact-map"
          title="AmCart location"
          src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d224345.83923192776!2d77.06889754725782!3d28.52758200617607!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x390cfd5b347eb62d%3A0x52c2b7494e204dce!2sNew%20Delhi%2C%20Delhi!5e0!3m2!1sen!2sin!4v1700000000000!5m2!1sen!2sin"
          loading="lazy"
          referrerPolicy="no-referrer-when-downgrade"
          allowFullScreen
        />

        <h2 className="contact-section-title">Leave A Reply</h2>

        <form className="reply-form" onSubmit={handleSubmit}>
          {success && <p className="reply-form__success">Your message has been sent successfully!</p>}
          {error && <p className="reply-form__error">{error}</p>}

          <div className="reply-form__fields">
            <input
              className="reply-form__input"
              type="text"
              placeholder="Name *"
              value={form.name}
              onChange={update("name")}
              required
            />
            <input
              className="reply-form__input"
              type="email"
              placeholder="E-mail *"
              value={form.email}
              onChange={update("email")}
              required
            />
            <input
              className="reply-form__input"
              type="text"
              placeholder="Enter your subject *"
              value={form.subject}
              onChange={update("subject")}
              required
            />
          </div>

          <textarea
            className="reply-form__textarea"
            placeholder="Your comment"
            value={form.comment}
            onChange={update("comment")}
          />

          <div className="reply-form__footer">
            <span className="reply-form__note">
              Your email address will not be published. Required fields are marked *
            </span>
            <button type="submit" className="reply-form__submit" disabled={submitting}>
              {submitting ? "Sending..." : "Post Comment"}
            </button>
          </div>
        </form>
      </div>

      {/* ——— Right Column ——— */}
      <aside className="contact-sidebar">
        <div>
          <h2 className="contact-section-title" style={{ marginTop: 0 }}>
            Contact Person
          </h2>
          <div className="contact-persons">
            {CONTACT_PERSONS.map((p) => (
              <div key={p.email} className="person-card">
                <div className="person-card__avatar">{p.initials}</div>
                <div className="person-card__info">
                  <span className="person-card__name">
                    {p.name} <span className="person-card__role">&ndash; {p.role}</span>
                  </span>
                  <span className="person-card__detail">
                    <span className="person-card__icon">&#9742;</span> {p.phone}
                  </span>
                  <span className="person-card__detail">
                    <span className="person-card__icon">&#9993;</span>{" "}
                    <a href={`mailto:${p.email}`}>{p.email}</a>
                  </span>
                </div>
              </div>
            ))}
          </div>
        </div>

        <div>
          <h2 className="contact-section-title" style={{ marginTop: 0 }}>
            Contact Details
          </h2>
          <div className="contact-details">
            <div className="contact-detail-row">
              <span className="contact-detail-row__icon">&#9742;</span>
              +91 11 4567 8900
            </div>
            <div className="contact-detail-row">
              <span className="contact-detail-row__icon">&#9990;</span>
              +91 98765 43210
            </div>
            <div className="contact-detail-row">
              <span className="contact-detail-row__icon">&#9993;</span>
              <a href="mailto:support@amcart.com">support@amcart.com</a>
            </div>
            {/* <div className="contact-detail-row">
              <span className="contact-detail-row__icon">&#9786;</span>
              AmCart_Official
            </div> */}
          </div>
        </div>
      </aside>
    </div>
  );
}
