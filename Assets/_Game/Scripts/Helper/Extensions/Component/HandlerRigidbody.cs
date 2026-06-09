using UnityEngine;

namespace Handler.Extensions
{
    public static class HandlerRigidbody
    {
        #region Velocity Extensions

        public static void ChangeDirection(this Rigidbody self, Vector3 direction)
        {
            self.linearVelocity = direction.normalized * self.linearVelocity.magnitude;
        }
        public static void NormalizeVelocity(this Rigidbody self, float magnitude = 1)
        {
            self.linearVelocity = self.linearVelocity.normalized * magnitude;
        }
        
        public static void SetVelocityX(this Rigidbody self, float x)
        {
            self.linearVelocity = new Vector3(x, self.linearVelocity.y, self.linearVelocity.z);
        }
        
        public static void SetVelocityY(this Rigidbody self, float y)
        {
            self.linearVelocity = new Vector3(self.linearVelocity.x, y, self.linearVelocity.z);
        }
        
        public static void SetVelocityZ(this Rigidbody self, float z)
        {
            self.linearVelocity = new Vector3(self.linearVelocity.x, self.linearVelocity.y, z);
        }
        
        public static void SetVelocity(this Rigidbody self, float x, float y, float z)
        {
            self.linearVelocity = new Vector3(x, y, z);
        }

        #endregion

        #region Rotation Extensions

        public static void SetRotationX(this Rigidbody self, float x)
        {
            self.rotation = Quaternion.Euler(x, self.rotation.eulerAngles.y, self.rotation.eulerAngles.z);
        }
        
        public static void SetRotationY(this Rigidbody self, float y)
        {
            self.rotation = Quaternion.Euler(self.rotation.eulerAngles.x, y, self.rotation.eulerAngles.z);
        }
        
        public static void SetRotationZ(this Rigidbody self, float z)
        {
            self.rotation = Quaternion.Euler(self.rotation.eulerAngles.x, self.rotation.eulerAngles.y, z);
        }
        
        public static void SetRotation(this Rigidbody self, float x, float y, float z)
        {
            self.rotation = Quaternion.Euler(x, y, z);
        }

        #endregion
        
        #region Rigidbody Utils Extensions
        
        public static void SetConstraints(this Rigidbody self, bool x, bool y, bool z)
        {
            self.constraints = RigidbodyConstraints.None;
            if (x) self.constraints |= RigidbodyConstraints.FreezePositionX;
            if (y) self.constraints |= RigidbodyConstraints.FreezePositionY;
            if (z) self.constraints |= RigidbodyConstraints.FreezePositionZ;
        }
        
        public static void SetCollisionDetectionMode(this Rigidbody self, CollisionDetectionMode mode)
        {
            self.collisionDetectionMode = mode;
        }
        
        public static void SetInterpolation(this Rigidbody self, RigidbodyInterpolation interpolation)
        {
            self.interpolation = interpolation;
        }
        
        public static void StopRigidbody(this Rigidbody self)
        {
            self.linearVelocity = Vector3.zero;
            self.angularVelocity = Vector3.zero;
        }
        
        public static void FreezeRigidbody(this Rigidbody self)
        {
            self.constraints = RigidbodyConstraints.FreezeAll;
        }
        
        public static void UnfreezeRigidbody(this Rigidbody self)
        {
            self.constraints = RigidbodyConstraints.None;
        }
        
        public static void AddForceToReachPosition(this Rigidbody rb, Vector3 targetPosition, float timeToReach)
        {
            // Calculate the necessary displacement to reach the target position
            Vector3 desiredDisplacement = targetPosition - rb.position;

            // Calculate the target velocity needed to reach the target position within the given time
            Vector3 targetVelocity = desiredDisplacement / timeToReach;

            // Calculate the necessary velocity change to reach the target velocity
            Vector3 desiredVelocityChange = targetVelocity - rb.linearVelocity;

            // Calculate the force required to achieve the target velocity
            Vector3 force = (desiredVelocityChange / timeToReach) * rb.mass;

            // Apply the calculated force
            rb.AddForce(force, ForceMode.Force);
        }
        
        public static void AddForceToReachVelocity(this Rigidbody rb, Vector3 targetVelocity, float timeToReach)
        {
            // Calculate the necessary force to change velocity within the given time
            Vector3 desiredVelocityChange = targetVelocity - rb.linearVelocity;
            Vector3 force = (desiredVelocityChange / timeToReach) * rb.mass;

            // Apply the calculated force
            rb.AddForce(force, ForceMode.Force);
        }
        
        public static void AddTorqueToReachRotation(this Rigidbody rb, Vector3 targetEulerAngles, float timeToReach)
        {
            // Convert target Euler angles to a Quaternion
            Quaternion targetRotation = Quaternion.Euler(targetEulerAngles);

            // Calculate the necessary angular displacement to reach the target rotation
            Quaternion desiredRotation = targetRotation * Quaternion.Inverse(rb.rotation);
            Vector3 desiredEulerRotation = desiredRotation.eulerAngles * Mathf.Deg2Rad;

            // Calculate the target angular velocity needed to reach the target rotation within the given time
            Vector3 targetAngularVelocity = desiredEulerRotation / timeToReach;

            // Calculate the necessary angular velocity change to reach the target angular velocity
            Vector3 desiredAngularVelocityChange = targetAngularVelocity - rb.angularVelocity;

            // Calculate the torque required to achieve the target angular velocity
            Vector3 torque = Vector3.Scale(desiredAngularVelocityChange / timeToReach, rb.inertiaTensor);

            // Apply the calculated torque
            rb.AddTorque(torque, ForceMode.Force);
        }
        
        #endregion
    }
}