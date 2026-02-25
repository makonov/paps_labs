import React, { useState } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Login from './pages/Login';
import Conferences from './pages/Conferences';
import Talks from './pages/Talks';
import Speakers from './pages/Speakers';
import Navbar from './components/Navbar';

const App: React.FC = () => {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem('token'));
  const [role, setRole] = useState<string | null>(() => localStorage.getItem('role')); // <-- добавили role

  // Функция выхода
  const logout = () => {
    setToken(null);
    setRole(null); // очищаем роль
    localStorage.removeItem('token');
    localStorage.removeItem('role'); 
  };

  return (
    <Router>
      <Navbar token={token} logout={logout} role={role} />
      <Routes>
        <Route path="/" element={<Conferences token={token} role={role} />} />
        <Route path="/login" element={<Login setToken={setToken} setRole={setRole} />} />
        <Route path="/conferences/:id/talks" element={<Talks token={token} role={role} />} />
        <Route path="/speakers" element={<Speakers token={token} role={role} />} />
      </Routes>
    </Router>
  );
};

export default App;
