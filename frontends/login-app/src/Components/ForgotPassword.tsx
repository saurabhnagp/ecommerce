import { useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";

function isValidEmail(value: string): boolean {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(value.trim());
}

function isValidPhone(value: string): boolean {
  const digitsOnly = value.replace(/\D/g, "");
  return digitsOnly.length >= 10 && digitsOnly.length <= 15;
}

function validateEmailOrPhone(value: string): { valid: boolean; message: string } {
  const trimmed = value.trim();
  if (!trimmed) return { valid: false, message: "Email or phone number is required" };
  if (isValidEmail(trimmed)) return { valid: true, message: "" };
  if (isValidPhone(trimmed)) return { valid: true, message: "" };
  return { valid: false, message: "Enter a valid email address or phone number" };
}

function ForgotPassword() {
  const navigate = useNavigate();
  const [emailOrPhone, setEmailOrPhone] = useState("");
  const [errors, setErrors] = useState<Record<string, string>>({});

  const handleSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const validation = validateEmailOrPhone(emailOrPhone);
    if (!validation.valid) {
      setErrors({ emailOrPhone: validation.message });
      return;
    }
    setErrors({});
    // TODO: call API to send reset link/code; then go to change password
    navigate("/change-password", { state: { emailOrPhone: emailOrPhone.trim() } });
  };

  return (
    <div className="min-h-screen bg-gray-100 flex items-center justify-center p-4">
      <div className="w-full max-w-md bg-white rounded-lg border border-gray-300 shadow-sm p-6">
        <h1 className="text-xl font-semibold text-gray-900 mb-2">Forgot your password?</h1>
        <p className="text-gray-600 text-sm mb-4">Enter your email or phone number to reset your password.</p>

        <form onSubmit={handleSubmit} className="space-y-4">
          <input
            type="text"
            inputMode="email"
            placeholder="Email or Phone Number"
            value={emailOrPhone}
            onChange={(e) => {
              setEmailOrPhone(e.target.value);
              if (errors.emailOrPhone) setErrors((prev) => ({ ...prev, emailOrPhone: "" }));
            }}
            className={`w-full px-4 py-2.5 border rounded-lg text-gray-800 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-amber-500 focus:border-transparent ${
              errors.emailOrPhone ? "border-red-500" : "border-gray-300"
            }`}
            aria-invalid={!!errors.emailOrPhone}
          />
          {errors.emailOrPhone && <p className="text-sm text-red-500">{errors.emailOrPhone}</p>}

          <button
            type="submit"
            className="w-full py-3 bg-amber-600 text-white font-medium uppercase text-sm rounded-lg hover:bg-amber-700 transition-colors"
          >
            Send reset link
          </button>
        </form>

        <p className="mt-4 text-center">
          <Link to="/login" className="text-sm text-gray-600 hover:underline">
            Back to Login
          </Link>
        </p>
      </div>
    </div>
  );
}

export default ForgotPassword;
