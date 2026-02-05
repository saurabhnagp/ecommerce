import { useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate, useLocation } from "react-router-dom";

function ChangePassword() {
  const navigate = useNavigate();
  const location = useLocation();
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [errors, setErrors] = useState<Record<string, string>>({});

  const emailOrPhone = location.state?.emailOrPhone as string | undefined;

  const handleSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const newErrors: Record<string, string> = {};
    if (!newPassword.trim()) newErrors.newPassword = "New password is required";
    else if (newPassword.length < 6) newErrors.newPassword = "Password must be at least 6 characters";
    if (newPassword !== confirmPassword) newErrors.confirmPassword = "Passwords do not match";
    if (!confirmPassword.trim()) newErrors.confirmPassword = "Please confirm your password";

    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors);
      return;
    }
    setErrors({});
    // TODO: call API to update password
    navigate("/login");
  };

  return (
    <div className="min-h-screen bg-gray-100 flex items-center justify-center p-4">
      <div className="w-full max-w-md bg-white rounded-lg border border-gray-300 shadow-sm p-6">
        <h1 className="text-xl font-semibold text-gray-900 mb-2">Change password</h1>
        {emailOrPhone && (
          <p className="text-gray-600 text-sm mb-4">
            Set a new password for <span className="font-medium">{emailOrPhone}</span>
          </p>
        )}
        {!emailOrPhone && (
          <p className="text-gray-600 text-sm mb-4">Enter your new password below.</p>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <input
            type="password"
            placeholder="New Password"
            value={newPassword}
            onChange={(e) => {
              setNewPassword(e.target.value);
              if (errors.newPassword) setErrors((prev) => ({ ...prev, newPassword: "" }));
            }}
            className={`w-full px-4 py-2.5 border rounded-lg text-gray-800 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-amber-500 focus:border-transparent ${
              errors.newPassword ? "border-red-500" : "border-gray-300"
            }`}
            aria-invalid={!!errors.newPassword}
          />
          {errors.newPassword && <p className="text-sm text-red-500">{errors.newPassword}</p>}

          <input
            type="password"
            placeholder="Confirm New Password"
            value={confirmPassword}
            onChange={(e) => {
              setConfirmPassword(e.target.value);
              if (errors.confirmPassword) setErrors((prev) => ({ ...prev, confirmPassword: "" }));
            }}
            className={`w-full px-4 py-2.5 border rounded-lg text-gray-800 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-amber-500 focus:border-transparent ${
              errors.confirmPassword ? "border-red-500" : "border-gray-300"
            }`}
            aria-invalid={!!errors.confirmPassword}
          />
          {errors.confirmPassword && <p className="text-sm text-red-500">{errors.confirmPassword}</p>}

          <button
            type="submit"
            className="w-full py-3 bg-amber-600 text-white font-medium uppercase text-sm rounded-lg hover:bg-amber-700 transition-colors"
          >
            Change password
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

export default ChangePassword;
