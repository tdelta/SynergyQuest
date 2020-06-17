import React from 'react';
import { BarLoader } from 'react-spinners';

export class LoadingScreen extends React.Component<{}, {}> {
  render() {
    return (
      <div className='container'>
        <div className='columnContainer'>
          <p className='text'> Connecting </p>
          <BarLoader color='#fff' />
        </div>
      </div>
    );
  }
}
