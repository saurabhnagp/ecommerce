import { useEffect, useRef, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { clearSession, getStoredUser } from "../auth/storage";
import "./UserMenuDropdown.css";

type Props = {
  onLogout: () => void;
};

export function UserMenuDropdown({ onLogout }: Props) {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);
  const navigate = useNavigate();
  const user = getStoredUser();

  useEffect(() => {
    function onDoc(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    }
    document.addEventListener("click", onDoc);
    return () => document.removeEventListener("click", onDoc);
  }, []);

  const label = user?.name?.trim() || user?.email?.split("@")[0] || "Account";
  const initial = (label[0] ?? "?").toUpperCase();

  function logout() {
    clearSession();
    setOpen(false);
    onLogout();
    navigate("/");
  }

  return (
    <div className="user-menu" ref={ref}>
      <button
        type="button"
        className="user-menu__trigger"
        aria-expanded={open}
        aria-haspopup="true"
        onClick={(e) => {
          e.stopPropagation();
          setOpen((v) => !v);
        }}
      >
        <span className="user-menu__avatar" aria-hidden>
          {initial}
        </span>
        <span className="user-menu__name">{label}</span>
        <span className="user-menu__caret" aria-hidden>
          ▼
        </span>
      </button>
      {open && (
        <div className="user-menu__panel" role="menu">
          <Link to="/account/profile" className="user-menu__item" role="menuitem" onClick={() => setOpen(false)}>
            Profile
          </Link>
          <Link
            to="/account/change-password"
            className="user-menu__item"
            role="menuitem"
            onClick={() => setOpen(false)}
          >
            Change password
          </Link>
          {user?.role === "admin" && (
            <Link to="/admin" className="user-menu__item" role="menuitem" onClick={() => setOpen(false)}>
              Admin Panel
            </Link>
          )}
          <button type="button" className="user-menu__item user-menu__item--danger" role="menuitem" onClick={logout}>
            Log out
          </button>
        </div>
      )}
    </div>
  );
}
