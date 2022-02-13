import React, { useEffect, useState } from 'react';;
import axios from 'axios';
import './App.css';
import { Header, List } from 'semantic-ui-react';

function App() {
  const [activites, setActivities] = useState([]);
  useEffect(() => {
    axios.get('https://localhost:5001/api/activities').then(response => {
      console.log(response);
      setActivities(response.data);
    });
  }, []);

  return (
    <div>
      <Header as='h2' icon='users' content='Reactivities' />
      <List>
        {activites.map((activity: any) => (
            <List.Item key={activity.id}>
              {activity.title}
            </List.Item>
          ))}
      </List>
    </div>
  );
}

export default App;
