import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { login } from '../api';

interface Props {
  setToken: React.Dispatch<React.SetStateAction<string | null>>;
  setRole: React.Dispatch<React.SetStateAction<string | null>>;
}

const Login: React.FC<Props> = ({ setToken, setRole }) => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const navigate = useNavigate();

  const handleLogin = async () => {
    try {
      const data = await login(username, password); // { accessToken, role }
      setToken(data.accessToken);
      setRole(data.role);
      localStorage.setItem('token', data.accessToken);
      localStorage.setItem('role', data.role);
      navigate('/conferences');
    } catch {
      alert('Ошибка входа');
    }
  };

  return (
    <div>
      <h2>Вход</h2>
      <input placeholder="Логин" onChange={e => setUsername(e.target.value)} />
      <input type="password" placeholder="Пароль" onChange={e => setPassword(e.target.value)} />
      <button onClick={handleLogin}>Войти</button>
    </div>
  );
};

export default Login;