import styles from './RightButton.module.css'
import rightIcon from '../../../assets/right.png'

export interface RightButtonProps {
  onClick: () => void;
  isActive: boolean;
}

export const RightButton: React.FC<RightButtonProps> = ({onClick, isActive}) => {
	return (
		<button className={`${styles.button} ${isActive ? '' : styles.disabled}`} onClick={isActive ? onClick : () => {}}>
			<img src={rightIcon}/>
		</button>
	)
};
