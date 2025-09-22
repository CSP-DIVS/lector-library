import './Sidebar.css';

const Sidebar = ({ user, activeSection, onSectionChange }) => {
  const menuItems = getMenuItemsForRole(user.role);

  return (
    <aside className="sidebar">
      <div className="sidebar-header">
        <div className="logo">
          <div className="logo-icon">📚</div>
          <div className="logo-text">
            <div className="logo-title">Lector</div>
            <div className="logo-subtitle">Library System</div>
          </div>
        </div>
      </div>
      
      <nav className="sidebar-nav">
        <div className="nav-section">
          <div className="nav-section-title">Main</div>
          <button 
            className={`nav-item ${activeSection === 'home' ? 'active' : ''}`}
            onClick={() => onSectionChange('home')}
          >
            <svg className="nav-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/>
              <polyline points="9,22 9,12 15,12 15,22"/>
            </svg>
            Dashboard
          </button>
        </div>

        {menuItems.map((section) => (
          <div key={section.title} className="nav-section">
            <div className="nav-section-title">{section.title}</div>
            {section.items.map((item) => (
              <button
                key={item.key}
                className={`nav-item ${activeSection === item.key ? 'active' : ''}`}
                onClick={() => onSectionChange(item.key)}
              >
                <svg className="nav-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                  <path d={item.icon}/>
                </svg>
                {item.label}
              </button>
            ))}
          </div>
        ))}
      </nav>
      
      <div className="sidebar-footer">
        <div className="user-card">
          <div className="user-avatar-small">
            {user.username?.charAt(0).toUpperCase() || 'U'}
          </div>
          <div className="user-details">
            <div className="username">{user.username}</div>
            <div className="user-role">{user.role || 'Member'}</div>
          </div>
        </div>
      </div>
    </aside>
  );
};

const getMenuItemsForRole = (role) => {
  const baseItems = [
    {
      title: "Personal",
      items: [
        {
          key: 'profile',
          label: 'My Profile',
          icon: 'M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2'
        }
      ]
    }
  ];

  if (role === 'Administrator') {
    return [
      {
        title: "Management",
        items: [
          {
            key: 'user-management',
            label: 'User Management',
            icon: 'M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2'
          },
          {
            key: 'book-management',
            label: 'Book Management',
            icon: 'M2 3h6a4 4 0 0 1 4 4v14a3 3 0 0 0-3-3H2z'
          },
          {
            key: 'lending-reservation',
            label: 'Lending & Reservation',
            icon: 'M9 11H5a2 2 0 0 0-2 2v3a2 2 0 0 0 2 2h4l2 2h4a2 2 0 0 0 2-2v-3a2 2 0 0 0-2-2h-4l-2-2z'
          },
          {
            key: 'fines-payment',
            label: 'Fines & Payment',
            icon: 'M12 2v6l3-3 3 3V2M2 17h20v2H2'
          }
        ]
      },
      ...baseItems
    ];
  } else if (role === 'Librarian') {
    return [
      {
        title: "Operations",
        items: [
          {
            key: 'book-management',
            label: 'Book Management',
            icon: 'M2 3h6a4 4 0 0 1 4 4v14a3 3 0 0 0-3-3H2z'
          },
          {
            key: 'lending-reservation',
            label: 'Lending & Reservation',
            icon: 'M9 11H5a2 2 0 0 0-2 2v3a2 2 0 0 0 2 2h4l2 2h4a2 2 0 0 0 2-2v-3a2 2 0 0 0-2-2h-4l-2-2z'
          },
          {
            key: 'fines-payment',
            label: 'Fines & Payment',
            icon: 'M12 2v6l3-3 3 3V2M2 17h20v2H2'
          }
        ]
      },
      ...baseItems
    ];
  } else {
    return [
      {
        title: "Library",
        items: [
          {
            key: 'book-catalog',
            label: 'Book Catalog',
            icon: 'M2 3h6a4 4 0 0 1 4 4v14a3 3 0 0 0-3-3H2z'
          }
        ]
      },
      {
        title: "Services",
        items: [
          {
            key: 'lending-reservation',
            label: 'My Books & Reservations',
            icon: 'M9 11H5a2 2 0 0 0-2 2v3a2 2 0 0 0 2 2h4l2 2h4a2 2 0 0 0 2-2v-3a2 2 0 0 0-2-2h-4l-2-2z'
          },
          {
            key: 'fines-payment',
            label: 'My Fines & Payments',
            icon: 'M12 2v6l3-3 3 3V2M2 17h20v2H2'
          }
        ]
      },
      ...baseItems
    ];
  }
};

export default Sidebar;


