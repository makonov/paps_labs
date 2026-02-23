import React, { useState } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Login from './pages/Login';
import Conferences from './pages/Conferences';
import Talks from './pages/Talks';
import Speakers from './pages/Speakers';
import Navbar from './components/Navbar';

const App: React.FC = () => {
  const [token, setToken] = useState<string | null>(null);

  return (
    <Router>
      <Navbar token={token} />
      <Routes>
        <Route path="/" element={<Conferences token={token!} />} />
        <Route path="/login" element={<Login setToken={setToken} />} />
        <Route path="/conferences/:id" element={<Talks token={token!} />} />
        <Route path="/speakers" element={<Speakers token={token!} />} />
      </Routes>
    </Router>
  );
};

export default App;
