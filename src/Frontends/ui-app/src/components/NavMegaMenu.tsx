import { Link } from "react-router-dom";
import type { CategoryDto } from "../api/products";
import { categoryHref } from "../utils/categoryNav";

type Props = { root: CategoryDto };

/**
 * One top-level category (e.g. Men / Women): columns from first-level children.
 * - Child with nested subCategories → column title + links to grandchildren.
 * - Leaf child → column with title linking to that category.
 */
export function NavMegaMenu({ root }: Props) {
  const subs = root.subCategories ?? [];

  if (subs.length === 0) {
    return (
      <div className="mega-menu mega-menu--narrow">
        <div className="mega-menu__col">
          <h4 className="mega-menu__title">{root.name}</h4>
          <ul className="mega-menu__list">
            <li>
              <Link to={categoryHref(root.slug)} className="mega-menu__link">
                Shop {root.name}
              </Link>
            </li>
          </ul>
        </div>
      </div>
    );
  }

  return (
    <div className="mega-menu">
      {subs.map((sub) => {
        const grand = sub.subCategories ?? [];
        if (grand.length > 0) {
          return (
            <div key={sub.id} className="mega-menu__col">
              <h4 className="mega-menu__title">
                <Link to={categoryHref(sub.slug)} className="mega-menu__title-link">
                  {sub.name}
                </Link>
              </h4>
              <ul className="mega-menu__list">
                <li>
                  <Link to={categoryHref(sub.slug)} className="mega-menu__link">
                    All {sub.name}
                  </Link>
                </li>
                {grand.map((g) => (
                  <li key={g.id}>
                    <Link to={categoryHref(g.slug)} className="mega-menu__link">
                      {g.name}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          );
        }
        return (
          <div key={sub.id} className="mega-menu__col">
            <h4 className="mega-menu__title">
              <Link to={categoryHref(sub.slug)} className="mega-menu__title-link">
                {sub.name}
              </Link>
            </h4>
          </div>
        );
      })}
      <div className="mega-menu__col">
        <h4 className="mega-menu__title">All</h4>
        <ul className="mega-menu__list">
          <li>
            <Link to={categoryHref(root.slug)} className="mega-menu__link">
              All {root.name}
            </Link>
          </li>
        </ul>
      </div>
    </div>
  );
}
