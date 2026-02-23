import React, { useEffect, useState } from 'react';
import { getConferences } from '../api';
import { Link } from 'react-router-dom';

interface Conference {
  id: number;
  name: string;
  startDate: string;
  endDate: string;
  location: string;
}

interface ConferencesProps {
  token: string;
}

const Conferences: React.FC<ConferencesProps> = ({ token }) => {
  const [conferences, setConferences] = useState<Conference[]>([]);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetch = async () => {
      try {
        const data = await getConferences(token);
        setConferences(data);
      } catch {
        setError('Ошибка при загрузке конференций');
      }
    };
    fetch();
  }, [token]);

  return (
    <div>
      <h2>Конференции</h2>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      <ul>
        {conferences.map(c => (
          <li key={c.id}>
            <Link to={`/conferences/${c.id}`}>{c.name}</Link> — {c.startDate} – {c.endDate} — {c.location}
          </li>
        ))}
      </ul>
    </div>
  );
};

export default Conferences;