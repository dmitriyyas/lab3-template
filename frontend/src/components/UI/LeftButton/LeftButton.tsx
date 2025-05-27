import styles from './LeftButton.module.css'
import rightIcon from '../../../assets/left.png'

export interface LeftButtonProps {
  onClick: () => void;
  isActive: boolean;
}

export const LeftButton: React.FC<LeftButtonProps> = ({onClick, isActive}) => {
	return (
		<button className={`${styles.button} ${isActive ? '' : styles.disabled}`} onClick={isActive ? onClick : () => {}}>
			<img src={rightIcon}/>
		</button>
	)
};
