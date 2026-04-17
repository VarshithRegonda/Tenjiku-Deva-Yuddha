import { Redirect } from 'expo-router';

// This catch-all route ensures that if a user (or a cached browser tab) 
// tries to navigate to a messy route like /explore, they are 
// immediately redirected to the high-fidelity main dashboard.
export default function NotFound() {
  return <Redirect href="/" />;
}
