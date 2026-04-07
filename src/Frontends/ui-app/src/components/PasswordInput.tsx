import { useState } from "react";

type Props = Omit<React.InputHTMLAttributes<HTMLInputElement>, "type"> & {
  icon?: string;
};

export function PasswordInput({ icon = "\u{1F512}", className, ...rest }: Props) {
  const [visible, setVisible] = useState(false);

  return (
    <label className="field-wrap">
      <input {...rest} type={visible ? "text" : "password"} className={className} />
      <button
        type="button"
        className="field-toggle"
        onClick={() => setVisible((v) => !v)}
        aria-label={visible ? "Hide password" : "Show password"}
        tabIndex={-1}
      >
        {visible ? "\u{1F441}" : icon}
      </button>
    </label>
  );
}
