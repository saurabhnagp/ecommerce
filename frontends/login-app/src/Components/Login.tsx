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

function Login() {
  const navigate = useNavigate();
  const [emailOrPhone, setEmailOrPhone] = useState("");
  const [password, setPassword] = useState("");
  const [rememberMe, setRememberMe] = useState(false);
  const [errors, setErrors] = useState<{ emailOrPhone?: string; password?: string }>({});

  const handleSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const emailOrPhoneValidation = validateEmailOrPhone(emailOrPhone);
    const passwordError = !password.trim() ? "Password is required" : "";

    if (!emailOrPhoneValidation.valid || passwordError) {
      setErrors({
        emailOrPhone: emailOrPhoneValidation.valid ? undefined : emailOrPhoneValidation.message,
        password: passwordError || undefined,
      });
      return;
    }

    setErrors({});
    // TODO: call login API; for now just navigate
    navigate("/");
  };

  return (
    <div className="min-h-screen bg-gray-100 flex items-center justify-center p-4">
      <div className="w-full max-w-md bg-white rounded-lg border border-gray-300 shadow-sm overflow-hidden">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-200">
          <h1 className="text-xl font-semibold text-gray-900">Sign in</h1>
          <Link
            to="/"
            className="text-gray-500 hover:text-gray-700 text-2xl leading-none"
            aria-label="Close"
          >
            ×
          </Link>
        </div>

        <form onSubmit={handleSubmit} className="p-6">
          {/* Social login */}
          <p className="text-center text-sm text-gray-600 mb-4">Sign in using</p>
          <div className="flex gap-3 mb-6">
            <button
              type="button"
              className="flex-1 flex items-center justify-center gap-2 py-2.5 rounded bg-[#1877f2] text-white text-sm font-medium uppercase"
            >
              <span className="text-lg font-bold">f</span>
              Facebook
            </button>
            <button
              type="button"
              className="flex-1 flex items-center justify-center gap-2 py-2.5 rounded bg-[#1da1f2] text-white text-sm font-medium uppercase"
            >
              <span className="text-lg">𝕏</span>
              Twitter
            </button>
          </div>

          <div className="border-t border-gray-200 my-6" />

          {/* Email / Phone */}
          <div className="mb-4">
            <input
              type="text"
              inputMode="email"
              placeholder="Email / Phone Number"
              value={emailOrPhone}
              onChange={(e) => {
                setEmailOrPhone(e.target.value);
                if (errors.emailOrPhone) setErrors((prev) => ({ ...prev, emailOrPhone: undefined }));
              }}
              className={`w-full px-4 py-2.5 border rounded text-gray-800 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
                errors.emailOrPhone ? "border-red-500" : "border-gray-300"
              }`}
              aria-invalid={!!errors.emailOrPhone}
              aria-describedby={errors.emailOrPhone ? "email-error" : undefined}
            />
            {errors.emailOrPhone && (
              <p id="email-error" className="mt-1 text-sm text-red-500">
                {errors.emailOrPhone}
              </p>
            )}
          </div>

          {/* Password */}
          <div className="mb-4">
            <input
              type="password"
              placeholder="Password"
              value={password}
              onChange={(e) => {
                setPassword(e.target.value);
                if (errors.password) setErrors((prev) => ({ ...prev, password: undefined }));
              }}
              className={`w-full px-4 py-2.5 border rounded text-gray-800 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
                errors.password ? "border-red-500" : "border-gray-300"
              }`}
              aria-invalid={!!errors.password}
              aria-describedby={errors.password ? "password-error" : undefined}
            />
            {errors.password && (
              <p id="password-error" className="mt-1 text-sm text-red-500">
                {errors.password}
              </p>
            )}
          </div>

          {/* Remember me */}
          <label className="flex items-center gap-2 mb-4 cursor-pointer">
            <input
              type="checkbox"
              checked={rememberMe}
              onChange={(e) => setRememberMe(e.target.checked)}
              className="w-4 h-4 rounded border-gray-300 text-gray-700 focus:ring-blue-500"
            />
            <span className="text-sm text-gray-700">remember me</span>
          </label>

          {/* LOGIN button */}
          <button
            type="submit"
            className="w-full py-3 bg-black text-white font-medium uppercase text-sm rounded hover:bg-gray-800 transition-colors"
          >
            Login
          </button>

          {/* Forgot password */}
          <p className="text-center mt-3">
            <Link to="/forgot-password" className="text-sm text-amber-700 hover:underline">
              Forgot your password?
            </Link>
          </p>
        </form>

        <div className="border-t border-gray-200 px-6 py-4">
          <Link
            to="/register"
            className="block w-full py-3 text-center bg-amber-600 text-white font-medium uppercase text-sm rounded hover:bg-amber-700 transition-colors"
          >
            Register
          </Link>
        </div>
      </div>
    </div>
  );
}

export default Login;
