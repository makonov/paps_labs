import React from 'react';
import { Link } from 'react-router-dom';

interface NavbarProps {
  token: string | null;
}

const Navbar: React.FC<NavbarProps> = ({ token }) => {
  return (
    <nav>
      <Link to="/">Конференции</Link> | <Link to="/speakers">Спикеры</Link> |{' '}
      {token ? <span>Вход выполнен</span> : <Link to="/login">Войти</Link>}
    </nav>
  );
};

export default Navbar;