import { useState } from 'react';
import Sidebar from './ui/Sidebar';
import Header from './ui/Header';
import HomePage from './pages/HomePage';
import UserManagement from './pages/UserManagement';
import BookManagement from './pages/BookManagement';
import BookCatalog from './pages/BookCatalog';
import LendingReservation from './pages/LendingReservation';
import FinesPayment from './pages/FinesPayment';
import MyProfile from './MyProfile';
import './MainLayout.css';

const MainLayout = ({ user, onLogout }) => {
  const [activeSection, setActiveSection] = useState('home');

  const renderContent = () => {
    switch (activeSection) {
      case 'home':
        return <HomePage user={user} />;
      case 'user-management':
        return <UserManagement user={user} />;
      case 'book-management':
        return <BookManagement user={user} />;
      case 'book-catalog':
        return <BookCatalog user={user} />;
      case 'lending-reservation':
        return <LendingReservation user={user} />;
      case 'fines-payment':
        return <FinesPayment user={user} />;
      case 'profile':
        return <MyProfile user={user} />;
      default:
        return <HomePage user={user} />;
    }
  };

  return (
    <div className="main-layout">
      <Sidebar 
        user={user} 
        activeSection={activeSection}
        onSectionChange={setActiveSection} 
      />
      <div className="main-content">
        <Header user={user} onLogout={onLogout} />
        <main className="content-area">
          {renderContent()}
        </main>
      </div>
    </div>
  );
};

export default MainLayout;
