import React from 'react';
import { Link } from 'react-router-dom';

interface NavbarProps {
  token: string | null;
  role: string | null;
  logout: () => void;
}

const Navbar: React.FC<NavbarProps> = ({ token, logout }) => {
  return (
    <nav>
      <Link to="/">Конференции</Link> | <Link to="/speakers">Спикеры</Link> |{' '}
      {token ? (
        <button onClick={logout}>Выйти</button> // <-- кнопка выхода
      ) : (
        <Link to="/login">Войти</Link>
      )}
    </nav>
  );
};

export default Navbar;