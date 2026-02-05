import { useState } from "react";
import type { FormEvent } from "react";
import { Link } from "react-router-dom";

function Register() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [mobile, setMobile] = useState("");
  const [gender, setGender] = useState<"male" | "female" | "">("");
  const [errors, setErrors] = useState<Record<string, string>>({});

  const handleSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const newErrors: Record<string, string> = {};
    if (!email.trim()) newErrors.email = "Email is required";
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) newErrors.email = "Enter a valid email";
    if (!password.trim()) newErrors.password = "Password is required";
    if (!mobile.trim()) newErrors.mobile = "Mobile number is required";
    if (!gender) newErrors.gender = "Please select gender";

    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors);
      return;
    }
    setErrors({});
    // TODO: call registration API
  };

  return (
    <div className="min-h-screen bg-gray-100 flex items-center justify-center p-4">
      <div className="w-full max-w-md bg-white rounded-lg border border-gray-300 shadow-sm p-6">
        <form onSubmit={handleSubmit} className="space-y-4">
          <input
            type="email"
            placeholder="Your Email Address"
            value={email}
            onChange={(e) => {
              setEmail(e.target.value);
              if (errors.email) setErrors((prev) => ({ ...prev, email: "" }));
            }}
            className={`w-full px-4 py-2.5 border rounded-lg text-gray-800 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-amber-500 focus:border-transparent ${
              errors.email ? "border-red-500" : "border-gray-300"
            }`}
            aria-invalid={!!errors.email}
          />
          {errors.email && <p className="text-sm text-red-500">{errors.email}</p>}

          <input
            type="password"
            placeholder="Choose Password"
            value={password}
            onChange={(e) => {
              setPassword(e.target.value);
              if (errors.password) setErrors((prev) => ({ ...prev, password: "" }));
            }}
            className={`w-full px-4 py-2.5 border rounded-lg text-gray-800 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-amber-500 focus:border-transparent ${
              errors.password ? "border-red-500" : "border-gray-300"
            }`}
            aria-invalid={!!errors.password}
          />
          {errors.password && <p className="text-sm text-red-500">{errors.password}</p>}

          <input
            type="tel"
            placeholder="Mobile Number (For order status updates)"
            value={mobile}
            onChange={(e) => {
              setMobile(e.target.value);
              if (errors.mobile) setErrors((prev) => ({ ...prev, mobile: "" }));
            }}
            className={`w-full px-4 py-2.5 border rounded-lg text-gray-800 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-amber-500 focus:border-transparent ${
              errors.mobile ? "border-red-500" : "border-gray-300"
            }`}
            aria-invalid={!!errors.mobile}
          />
          {errors.mobile && <p className="text-sm text-red-500">{errors.mobile}</p>}

          <div className="flex items-center gap-4">
            <span className="text-sm text-gray-700">I'm a</span>
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="radio"
                name="gender"
                value="male"
                checked={gender === "male"}
                onChange={() => {
                  setGender("male");
                  if (errors.gender) setErrors((prev) => ({ ...prev, gender: "" }));
                }}
                className="w-4 h-4 text-amber-500 focus:ring-amber-500"
              />
              <span className="text-sm text-gray-700">Male</span>
            </label>
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="radio"
                name="gender"
                value="female"
                checked={gender === "female"}
                onChange={() => {
                  setGender("female");
                  if (errors.gender) setErrors((prev) => ({ ...prev, gender: "" }));
                }}
                className="w-4 h-4 text-amber-500 focus:ring-amber-500"
              />
              <span className="text-sm text-gray-700">Female</span>
            </label>
          </div>
          {errors.gender && <p className="text-sm text-red-500">{errors.gender}</p>}

          <button
            type="submit"
            className="w-full py-3 bg-amber-600 text-white font-medium uppercase text-sm rounded-lg hover:bg-amber-700 transition-colors"
          >
            Register
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

export default Register;
